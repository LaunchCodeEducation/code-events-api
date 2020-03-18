- create vm
- install docker
    - run mysql locally
- create key vault
- assign / confirm identity for vm
    - get object ID
- give identity access to keyvault
    - `az keyvault set-policy --name '<YourKeyVaultName>' --object-id <VMSystemAssignedIdentity> --secret-permissions get list` 
- create app configuration in portal
    - https://docs.microsoft.com/en-us/azure/azure-app-configuration/quickstart-aspnet-core-app?tabs=core3x
- can not use default ports
    - must update in appsettings.json (Kestrel entry)
    - open port 80 security setting
- 

## Migrations
- from migration script (dump into mysql?)
- separate project? other way?

## Keystore
1. hardcode the connection string
2. dotnet CLI user-secrets and GetConnectionString()
3. implement keystore access for deployment

## AD B2C
- create resource
    - AD b2c -> new tenant
        - name: <name>-ms-camp
        - domain: <name>mscamp
- manage b2c
- add application
    - name: code-events
    - include web app: yes  
    - allow implicit: no
    - reply URL (enter two):
        - https://localhost:5001/oauth/success (for local dev)
        - !! return later with deployment callback uri !!
    - takes a minute to show the new app (no refresh button)
- DONT FORGET TO LINK TO SUBSCRIPTION
    - TODO: fresh instructions
- properties
    - application (client id) id: 06eb34fd-455b-4084-92c3-07d5389e6c15
- application > keys
    - generate key (client secret) -> copy the key
        - x-TUFqf30gPfOdtPmT7(^ap0
- ?api access > scopes?
- top bar (azure ad b2c tab)
    - things to show 
        - identity providers
            - show where to add other providers (email default)
        - company branding (customizing auth view)
        - users -> activity
        - users -> user settings -> users can register apps??
            - TODO: confirm if this should be off
        - user attributes -> add
- user flows -> create user flow (recommended tab)
- repeat for each of the flows (signup/signin, editing, reset)
    - ?? all or just some of these?
- options
    - name: code_events_signup_signin (or flow type, snakecase)
    - email
    - MFA: disabled (but explain what it is?)
    - show more 
        - city, display name (username), email, state
            - collect/return all except email (explain why)
    
    
# flow

feel foreign because WE are the PROVIDER. they are not some third party 
provider (relative to us as the "primary"). 
confusing: we use our client to contact our provider to contact our api 
clarification:
    - not a single client and API directly connected to each other
    - need to think in terms of clients and microservices (plural!)
        - most small scale projects the client and API are 1:1
        - ADB2C is about multiple clients and multiple microservices bound by
         a common provider
            - think in terms of this mindset
            - a central authority that manages user identity across 
            microservices / gateways
- use case for ADB2C:     
    - [webapp] frontend/backend (multi or single host, arbitrary)
    - [provider] ADB2C ("tenant" that coordinates webapps with API 
    services 
    under a single identity)
    - [service(s)] APIs (microservices within the "tenant" network)
- otherwise you just handle auth within your app (no need for ADB2C)
    - requires 2 discreet services that need to be coordinated by your own 
    provider authority
         
        
[client] html -> button [github provider page]
[provider] html -> redirected [client]
[client] html -> (qs: code) -> [API]
[API] code -> code + client id/secret + ... -> [provider]
[provider] code + ... -> access token -> [API]
[API] stores access token -> signs cookie / personal JWT -> [client]
[client] uses cookie for future requests  -> [API]
[API] looks up from session -> uses access token
-> [provider] -> [API] -> [client]