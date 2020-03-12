TODO: review and convert to OpenAPI Swagger

# endpoints

# Event

> DB table: events

```
id: bigint
title: vc
start: datetime
description: vc
```

> JSON Event

```
{
  id
  title
  start
  description
  links:
    owner
    images
}
```

> JSON PartialEvent

```
{
  id
  title
  start
}
```

> EventRoles (enum)

- auth by role: Event.id = EventMember.event_id + @user = EventMember.user_id ->
- OWNER: EventMember.role = OWNER
- MEMBER: EventMember.role = MEMBER

```
{
  OWNER
  MEMBER
}
```

- collection: /events
  - GET: list of all events
    - res body: `PartialEvent[]`
  - @POST: create a new event
    - req
      - headers: auth with AAD
        - @user = new event owner
      - body: `{ title, description, start (date + time ISOString) }`
    - res: 201
      - body: url of new event resource
- resource: /events/{id}
  - @GET
    - res body: `Event`
  - @DELETE
    - ROLES: OWNER

## Member

> DB table: event_members

```
user_id: bigint, FK: users
event_id: bigint, FK: events
role: member_roles enum
```

- collection: /events/members
  - @GET: list of event members
    - res body: `User[]`
    - ROLES: MEMBER, OWNER
      - @user is a member or owner
  - @POST: join event
    - req
      - headers: auth with AAD
        - @user = new event member
    - res: 204

## Event Image

> DB table: event_images

```
event_id: bigint, FK: events
image_id: bigint, FK: images
```

> JSON EventImage

```
{
  id
  url
  links:
    tags
    uploader
}
```

- collection: /events/{id}/images
  - @GET: list of all event images
    - res body: `Image[]`
    - ROLES: MEMBER, OWNER
      - @user must be owner or member
  - @POST: upload an event image (multipart file)
    - ROLES: MEMBER, OWNER
      - @user must be owner or member

TODO: discuss use of tags. nice to have but maybe over complicates the goal (using AAD, blobs/ACL)

# Tag

> DB table: tags

```
id
name
```

> JSON Tag

```
{
  id
  name
  links:
    events
    images
}
```

- collection: /tags
  - GET: list of all tags separated by events and public images
    - res body: `Tag[]`

## EventTags

> DB table: event_tags

```
tag_id: bigint, FK: tags
event_id: bigint, FK: events
```

> JSON

- collection: /events/{id}/tags
  - GET: list of tags for the event
  - @POST: add a tag to an event
    - ROLES: OWNER
  - @DELETE: remove a tag from an event
    - ROLES: OWNER

# User

- collection: /users
- resource: /users/{username}
  - GET
  - POST

## UserImage

> DB table: user_images

```
user_id
image_id

```

# Image

> DB table: images

```
id
path
uploader
access: IMAGE_ACCESS enum
```

- collection: /users/{id}/images
  - @GET
  - @POST
- resource: /users/{id}/images/{id}
  - @GET: image blob URL with AAD RBAC based
    - IMAGE_ACCESS
      - PUBLIC: any @user
      - PRIVATE: owner only
        - @user.id = Image.uploader_id
      - EVENT: event member
        - Image.id = EventImage.image_id -> EventImage.event_id + @user.id = Member
  - @PATCH: update Image.image_access -> update blob RBAC
    - owner only
  - @DELETE
    - owner only

## ImageTag

- collection: /images/{uuid}/tags
  - GET
  - POST
  - DELETE

# Image Blob Storage

- api storage account
  - /events: all images (public and private) uploaded to events
    - /public: all public images uploaded to events
      - /{image_id}: image file
        - url: https://{storage-account}.blob.core.windows.net/public/{image_id}
    - /{event_id}: images only event members can view
      - /{image_id}: image file
        - url: https://{storage-account}.blob.core.windows.net/events/{event_id}/{image_id}?{SAS}
      - in our DB image access level controls whether SAS is used
        - public: no SAS
          - blob RBAC update to public read
        - private: require SAS
          - blob RBAC restricts to requiring SAS at /events/{event_id}
          - one per event member (stored in event_members entry)
          - user AAD signature scoped to the /events/{event_id}/
        - when toggling
          - TODO: confirm RBAC can be updated from public to scoped
          - update RBAC

URL[]

MVP: events, users, members, event_images bonus: tags

API: GET, POST, PATCH, DELETE

SSR: GET are rendered, other calls made to API

{ is_owner: boolean, url }[]

> roles

- images
  - public
  - event
  - ownership
    - user owner

# Image Blob Flow

> user joins event

- @user POST /events/{event_id}/members
  - create SAS for user
    - scope: to /events/{event_id}
    - permissions: /blobs/read, /blobs/add/action
  - create event_members entry
    - event_id
    - user_id
    - access_token: generate SAS for user scoped to /events/{event_id}

> user accesses images for an event

- TODO: consider
  - every time accessed test the event_member.access_token + refresh if expired
    - what is the test?
  - non expiring
    - note about security implications
- look up event_images
- generate URLs
  - event_images.path + event_member.access_token
- return list of urls (SAS and public)
- client side map over urls list -> img src = url

# Content

- do not cover REST
  - OpenAPI spec / Swagger
    - not a swagger lesson
  - skeleton structure and docs
- key storage
  - AAD creds
  - DB creds
- linux vm
  - golden image
    - show annotated script / setup process
  - RDS analog for DB
    - subnets (steps but not teaching)
  - SSH/SCP
    - do not discuss
    - black box (let azure manage everything)
  - no load balancing / reverse proxy
- next steps
  - security
  - scaling (horizontal/vertical)
  - load balancing
  -

structure

- mostly tutorial style
- conceptual sections
  - cloud computing
  -
- sections
  -
