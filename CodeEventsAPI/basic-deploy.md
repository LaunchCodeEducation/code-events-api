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
    - TODO: fresh intstructions
- properties
    - application id: 06eb34fd-455b-4084-92c3-07d5389e6c15
- application > keys
    - generate key -> copy the key
        - x-TUFqf30gPfOdtPmT7(^ap0
- ?api access > scopes?
- top bar (azure ad b2c tab)
    - things to show 
        - identity providers
            - show where to add other providers (email default)
        - company branding (customizing auth view)
        - users -> activity
        - users -> user settings -> users can register apps??
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
    