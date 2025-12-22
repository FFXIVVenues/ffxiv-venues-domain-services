# FFXIV Venues API

This repository contains the backend services for the FFXIV Venues platform.

## Services

### ApiGateway
The main API gateway service that provides RESTful endpoints for the FFXIV Venues platform. It handles:
- Creating venues
- Querying venues
- Updating venues
- Deleting venues
- Approving venues
- Flagging venues
- Subscribing to venue updates

Full API documentation be found at https://api.ffxivvenues.com/scalar/

### FlagService
The background domain service that owns content moderation flagging.

### OGCardService
Lightweight web service that generates Open Graph cards for venues, handled embedded web requests from Discord, Twitter, Facebook, LinkedIn and several other social media platforms.
