## Implementation notes
- API versioning added (v1)
- POST Request validated using Fluent Validation
- MediatR used to route the GET / POST requests to query / command handlers
- Refit is used to auto generate a REST client for the acquiring bank API
- Security features
	 - authorization scheme to allow user to enter a merchant id to simulate an authentication flow
	 - the GET payment endpoint checks the payment requested belongs to the merchant. Otherwise a NotFound is returned
- Idempotent requests (optional) 
     - The client may include an idempotency key in the request header to ensure the same POST request is not processed more than once
     - The merchant id is prefixed to the idempotency key to allow different merchants to use the same idempotency key
- Swagger UI configured with Authorize button which opens a dialog to enter the Authorization header value (merchant id))
	 
## Assumptions
- GET payment returns a masked card number (not just the last four card digits)
- No validation errors are returned when the POST request fails validation; only the phrase "Rejected"