# Requirements Workup

## Elicitation

1. Is the goal or outcome well defined?  Does it make sense?
   - Yes it is well defined and has good structure.
2. What is not clear from the given description?
   - How the scoring algo will work and be implemented. 
3. How about scope?  Is it clear what is included and what isn't?
   -  It is clear because it is structured like epics and stories
4. What do you not understand?
* Technical domain knowledge
    - How the encryption and auth with Plaid will work and how to get started
* Business domain knowledge
  - How to go about compliance for a full on deployment
1. Is there something missing?
   - What ui choices we want to make our standard. 
2. Get answers to these questions.

## Analysis

Go through all the information gathered during the previous round of elicitation.  

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.
# Entity Constraints & Limitations

## Users
- username: 3-20 chars, alphanumeric + underscores
- email: valid email format, unique
- password_hash: bcrypt, min length 8 chars
- is_verified: boolean (Plaid connection status)
- is_admin: boolean (special privileges)
- net_worth_visible: boolean (privacy toggle)
- plaid_token: encrypted string
- profile_image_url: max 2MB images, jpg/png only

## Posts
- title: 5-100 chars
- content: max 5000 chars
- image_urls: max 4 images
- upvotes/downvotes: integer, non-negative
- tags: 1-5 tags required per post

## Comments
- content: max 1000 chars
- upvotes/downvotes: integer, non-negative

## Tags
- name: 2-30 chars, unique
- type: enum ('Stock', 'Category')
- Stock tags: must start with '$'
- Category tags: predefined list (Gain, Loss, DD, News, etc.)

## Positions (Plaid-verified)
- quantity: decimal(18,8)
- prices: decimal(18,2)
- type: enum ('Stock', 'Option')
- ticker_symbol: 1-5 chars, uppercase

## Relationships
- User can follow unlimited users
- User can block unlimited users
- Post must have one author
- Post can have unlimited comments
- Post must have 1-5 tags
- Comment must have one author and one post
- Position must belong to one user

## Activities
- Rate limiting: max 100 requests/min per user
- Post creation: max 10/day for new users
- Comment creation: max 50/day per user
- Voting: one vote per post/comment per user
  
2. Do they work together or are there some conflicting requirements, specifications or behaviors?
   - Being able to hide portfolio amount goes against our platform and what it stands for, the rate limiting can interfere with live data and news like we want 
3. Have you discovered if something is missing?
   - Security, what a admin can do, error handling
4. Return to Elicitation activities if unanswered questions remain.


## Design and Modeling
Our first goal is to create a **data model** that will support the initial requirements.

1. Identify all entities;  for each entity, label its attributes; include concrete types
2. Identify relationships between entities.  Write them out in English descriptions.
3. Draw these entities and relationships in an _informal_ Entity-Relation Diagram.
4. If you have questions about something, return to elicitation and analysis before returning here.

## Analysis of the Design
The next step is to determine how well this design meets the requirements _and_ fits into the existing system.

1. Does it support all requirements/features/behaviors?
    * For each requirement, go through the steps to fulfill it.  Can it be done?  Correctly?  Easily?
2. Does it meet all non-functional requirements?
    * May need to look up specifications of systems, components, etc. to evaluate this.

