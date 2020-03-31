#! /usr/bin/env bash

# -- env vars --

# nginx
tls_key_path=/etc/nginx/external/key.pem
tls_cert_path=/etc/nginx/external/cert.pem

# api
api_service_user=api-user
api_working_dir=/opt/code-events-api/deploy

# -- end env vars --

# -- set up user and directories --
useradd -M "$api_service_user" -N -g student
mkdir /opt/code-events-api

chmod 770 /opt/code-events-api/
chown "$api_service_user":student /opt/code-events-api/

mkdir -p /etc/nginx/external

# -- end set up --

# -- install dependencies --

# install microsoft key and package repo for dotnet
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# install pre-req for ms packages
apt update -y && \
apt install -y apt-transport-https && \
apt update -y

# install dotnet runtime
# apt install -y dotnet-runtime-3.1 
apt install -y aspnetcore-runtime-3.1

# install nginx
apt install -y nginx

# -- end install dependencies --

# -- generate self signed cert

openssl req -x509 -newkey rsa:4086 \
-subj "/C=US/ST=Missouri/L=St. Louis/O=The LaunchCode Foundation/CN=localhost" \
-keyout "$tls_key_path" \
-out "$tls_cert_path" \
-days 3650 -nodes -sha256

# -- end self signed cert --

# -- generate conf files --

# nginx conf
cat << EOF > /etc/nginx/nginx.conf
events {}
http {
  # proxy settings
  proxy_redirect          off;
  proxy_set_header        Host \$host;
  proxy_set_header        X-Real-IP \$remote_addr;
  proxy_set_header        X-Forwarded-For \$proxy_add_x_forwarded_for;
  proxy_set_header        X-Forwarded-Proto \$scheme;
  client_max_body_size    10m;
  client_body_buffer_size 128k;
  proxy_connect_timeout   90;
  proxy_send_timeout      90;
  proxy_read_timeout      90;
  proxy_buffers           32 4k;

  limit_req_zone \$binary_remote_addr zone=one:10m rate=5r/s;
  server_tokens  off;

  sendfile on;
  keepalive_timeout   29; # Adjust to the lowest possible value that makes sense for your use case.
  client_body_timeout 10; client_header_timeout 10; send_timeout 10;

  upstream api{
    server localhost:5000;
  }

  server {
    listen     *:80;
    add_header Strict-Transport-Security max-age=15768000;
    return     301 https://\$host\$request_uri;
  }

  server {
    listen                    *:443 ssl;
    server_name               codeeventsapi.com;
    ssl_certificate           $tls_cert_path;
    ssl_certificate_key       $tls_key_path;
    ssl_protocols             TLSv1.1 TLSv1.2;
    ssl_prefer_server_ciphers on;
    ssl_ciphers               "EECDH+AESGCM:EDH+AESGCM:AES256+EECDH:AES256+EDH";
    ssl_ecdh_curve            secp384r1;
    ssl_session_cache         shared:SSL:10m;
    ssl_session_tickets       off;
    ssl_stapling              on; #ensure your cert is capable
    ssl_stapling_verify       on; #ensure your cert is capable

    add_header Strict-Transport-Security "max-age=63072000; includeSubdomains; preload";
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;

    #Redirects all traffic
    location / {
      proxy_pass http://api;
      limit_req  zone=one burst=10 nodelay;
    }
  }
}
EOF

# generate API unit file
cat << EOF > /etc/systemd/system/code-events-api.service
[Unit]
Description=Code Events API

[Service]
User=$api_service_user
WorkingDirectory=$api_working_dir
SyslogIdentifier=code-events-api
ExecStart=/usr/bin/dotnet ${api_working_dir}/CodeEventsAPI.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

# -- end generate conf --


# -- enable --

# enable nginx
service enable nginx

# enable api
service enable code-events-api

# -- end enable --