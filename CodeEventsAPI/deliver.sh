#! /usr/bin/env bash

set -ex

# env vars
local_publish_dir="$(pwd)/bin/deploy"
remote_target_dir=/opt/code-events-api

# publish the application
dotnet publish -c Release -o "$local_publish_dir"

# -- deliver to VM --

printf "\n\nfollow the instructions to deliver the project to your VM\n\n"

# prompt for SSH key
read -p "what is your SSH key name (assumes location in ~/.ssh/<key name>)? " ssh_key_name
ssh_key_path="~/.ssh/${ssh_key_name}"

if [[ -e $ssh_key_path ]]; then
echo "file $ssh_key_path does not exist. check that the key exists then run the script again"
exit 1
fi

# prompt for VM credentials
read -p "what is your VM shell username?: " vm_username
read -p "what is your VM IP address?: " vm_ip_address

scp_target="${vm_username}@${vm_ip_address}:${remote_target_dir}"

# SCP files to VM
scp -r -i "$ssh_key_path" "$local_publish_dir" "$scp_target"

echo "upload complete"