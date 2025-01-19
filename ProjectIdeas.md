# Projects in no particular order
Project System and Network Architecture: [Miro Link](https://miro.com/welcomeonboard/RE44Tm12anZwMXNGd3l1RWZOclM0Z0ZPSm0wTEw5MXQ3TWp3OWFCQjFCcEJqc2x4VytkRXB2VXIvdEtHc3BwWi9nbVZLQXJSWjAza2FIZVl1dk1CVWxjSnJLQlhNTE15ZUVxM3ZQN2xENlcyMHVjMTIvcHNCOTNsdStCd01zenchZQ==?share_link_id=321696217431)
1. ### Too long of a title but this is for a school project so the fact that its long is okay were helping to hit a non existent word quota
A RPG turn based combat using scenes to depict various battlefields  and different non-combative zones such as places to regain expended resources or obtain new ones. Using a database to fetch and store player data and records and a live service API such as websocket to allow for two way communication between the server and the client to allow for live sessions with other players.

Vision Statement:

"Made Virtual Combat" is a turn-based RPG designed to provide an engaging, story-driven experience for single players or groups of up to four, where every run feels unique and rewarding. Through strategic turn-based combat, players submit actions to a server for simultaneous resolution, prioritizing speed and move order to create dynamic encounters. Featuring a robust "Build" system, players can customize their characters using loot and items, synergizing abilities to adapt to increasing challenges. The game introduces a dynamic event system that interweaves environmental effects, such as weather or disasters, impacting gameplay and offering preparation opportunities. With an integrated achievement system, players can track personal and global milestones, fostering a sense of community and competition. While inspired by games like Divinity: Original Sin and Slay the Spire, "Made Virtual Combat" stands out by combining collaborative storytelling, evolving challenges, and a global tracking system to ensure a continually fresh and immersive experience.

*User Actions*
Combat Interaction:
- Players can choose actions (attack, defend, use items, cast spells) during their turn.
- Submit actions to the server for resolution based on speed and priority.
- React to environmental effects and adapt their strategy dynamically.

Character Development:
- Create and customize characters, selecting class, skills, and starting equipment.
- Gain experience points to level up, unlocking new abilities and stat boosts.
- Equip, modify, and combine items for enhanced performance and synergy.

Collaborative Play:
- Form parties of up to four players for co-op adventures.
- Strategize and communicate with teammates during combat.
- Share loot and resources within the group.

Exploration and Story Engagement:
- Explore procedurally generated maps with unique biomes and themes.
- Interact with NPCs for lore, quests, and trade opportunities.
- Make story-driven choices that affect character relationships and world events.

Event Engagement:
- Respond to dynamic environmental effects (e.g., weather, disasters).
- Participate in random events with risk/reward scenarios.

Achievement and Tracking:
- Unlock and track personal milestones (e.g., defeating specific enemies, clearing a biome).
- Contribute to global achievements for community goals.
- Compare stats and performance with other players.

*Features*
Dynamic Turn-Based Combat:
- Simultaneous resolution of player and enemy actions for unpredictable encounters.
- Priority-based action resolution tied to speed stats or item bonuses.
- Special enemy types that require unique strategies or preparation.

Robust Build System:
- Dynamic synergy between class abilities, items, and environmental effects.
- Loot variety with elemental and status effects (fire, ice, poison, etc.).
- Customizable loadouts tailored to specific challenges.

Procedural Generation:
- Randomized maps with diverse biomes and themes for replayability.
- Varied layouts, enemy placements, and hidden treasures.

Event System:
- Weather effects (e.g., rain weakening fire, storms amplifying lightning damage).
- Random disasters that alter gameplay, such as landslides or sudden enemy ambushes.
- Quests tied to unique map conditions or biome themes.

Multiplayer Integration:
- Real-time or asynchronous co-op play with a streamlined interface.
- Server-hosted sessions for secure and consistent gameplay.
- Chat and quick-action communication features.

Progression and Achievements:
- Unlockable achievements that provide cosmetic rewards or minor boosts.
- Persistent global tracking of significant milestones (e.g., total enemies defeated).

Narrative Integration:
- Branching storylines influenced by player decisions.
- Dynamic interactions with recurring NPCs who remember past choices.
- Worldbuilding through interactive lore pieces and quests.

Player Support and Accessibility:
- Tutorial mode for onboarding new players.
- Adjustable difficulty settings to cater to various skill levels.
- Comprehensive in-game glossary for mechanics, status effects, and items.

*User Needs*
Engagement:
- Unique gameplay scenarios and fresh challenges to sustain interest.
- Meaningful story choices with tangible consequences.

Customization:
- Ability to tailor characters and strategies to match personal playstyles.

Collaboration:
- Seamless multiplayer experience for team-based challenges.

Progression:
- Clear goals and rewards that provide a sense of accomplishment.

Replayability:
- Procedural generation and randomized events for diverse runs.

Fairness:
- Balanced combat mechanics that reward strategy over randomness.

*System Needs*
Stable Infrastructure:
- Reliable server-side action resolution for multiplayer.
- Fast load times for procedural map generation.

Scalability:
- Support for expanding features, player base, and content updates.

Cross-Platform Accessibility:
- Compatibility with PCs, consoles, and potentially mobile devices.

Data Security:
- Protection for player accounts, progress, and global tracking data.

Analytics Integration:
- In-game statistics to monitor player trends and improve balancing.


2. ### TrustTrade

TrustTrade would be a social platform that revolutionizes how traders interact by requiring portfolio verification through Plaid integration (optional but highlighted). Users who connect their brokerage accounts get a "Verified Portfolio" badge, displaying their real-time portfolio value, current stock positions, option contracts, and quantity of holdings. This verification system creates a hierarchy of trust, where verified users' posts and analyses carry more weight than unverified accounts. The platform would feature a "Performance Score" that tracks verified users' trading history, win rates, and risk-adjusted returns, making it impossible to fake success. Users can create detailed trade journals with entry/exit points, reasoning, and outcome analysis, all backed by verified transaction data.

Vision Statement:

TrustTrade, To build an open and transparent trading community where everyone can learn, grow, and succeed together. By combining verified performance with accessible knowledge sharing, we're creating a space where both novice and experienced traders can collaborate, validate their strategies, and help each other achieve financial success. Our platform breaks down traditional barriers in trading, making proven strategies and real-time insights available to all. Yes there is a competitor that is only an app currently called “AfterHour”, we will have a news section and performance scores for both users and stocks potentially that’ll set us apart. Ours will also only be web based. Also guest accounts available with limited access(no live feed or access to our api queries, only viewing). 

Core User Needs:
- Trust and verification in trading community
- Reliable verification of legitimate traders
- Validation of performance claims
- Protection against misinformation
- Learning and understanding successful strategies
- Connecting with like-minded traders
- Collaborative learning opportunities

Portfolio Verification Features:
- Plaid integration for brokerage connections
- Verified badge system
- Performance tracking metrics
- Privacy controls for sensitive data
- Protection against fake success claims

Social Trading Features:
- Real-time position sharing
- Trade journaling capabilities
- Strategy discussion spaces
- Custom watchlists (potential feature)
- Real-time learning from other traders
- Strategy breakdowns from verified traders
- Historical trade analysis tools

Community Management:
- Reputation scoring system
- Content moderation tools
- Group creation functionality
- Private messaging
- Safe discussion spaces
- Real-time market updates
- Sentiment analysis tools
- Popular trader movement tracking

Technical Requirements:
- High availability with consistent uptime
- Real-time data processing
- Secure data storage
- Scalable infrastructure
- Robust data privacy measures
- Financial regulation compliance
- Low latency updates
- Support for high concurrent users
- Efficient data caching
- Mobile responsiveness

Integration with:
- Brokerage APIs
- Market data feeds
- Payment processors
- Authentication services