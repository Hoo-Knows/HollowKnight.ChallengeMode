# Hollow Knight Challenge Mode

Each boss arena in Godhome now has randomly selected **modifiers** that make the game more challenging in a variety of ways, such as spawning enemies, adding a new mechanic to juggle, disabling or nerfing certain attacks, etc. The further the player gets in a pantheon, the more modifiers there are, and certain bosses will have unique modifiers. The mod works in both Pantheons and Hall of Gods (highly suggested to practice the boss modifiers).

### Menu Options
- Number of modifiers: determines the number of modifiers active by default (1 to 5)
- Increment modifiers: determines how much the number of modifiers is incremented after each section in a pantheon (0 to 2)
- Guarantee modifier: guarantee that a modifier will appear
- Use logic: whether or not to use logic to prevent easy or unfair modifier combinations (ex. High Stress with Unfriendly Fire and A Fool's Errand)
- Use slowdown: whether or not to slow down the game when displaying modifiers
- Allow High Stress: whether or not to allow High Stress
- Allow non-unique modifiers: whether or not to allow regular modifiers on bosses that have unique modifiers
- Allow modifiers everywhere: whether or not to allow modifiers in any room, even outside Godhome
- Reset settings: figure it out

### Dependencies
- SFCore
- Satchel

## List of Modifiers
- High Stress: sets the player’s health to 1 for 5 seconds after they get hit
- Frail Shell: makes the player take 1 extra damage from all sources
- Adrenaline Rush: taking damage speeds up the game slightly, up to a max of 1.2 speed
- Aspid Rancher: failing to hit an enemy spawns a Primal Aspid
- Void Vision: reduces the player's field of vision
- Speedrunner’s Curse: removes Steady Body, equips Thorns of Agony, and makes the player take hazard damage when they try to Descending Dark
- Nail Only: disables spells
- Soul Master: gives the player the soul over time, but sets nail damage to 1
- Hungry Knight: drains soul over time, the player begins taking damage after soul is empty
- Unfriendly Fire: spawns a Grimmchild that attacks the player
- Ascension: Gorb occasionally teleports onto the player and fires a ring of spears
- Salubra’s Curse: unequips all charms
- Past Regrets: spawns an invincible Shade with no contact damage that slashes at the player and copies any spell the player uses
- Infected Wounds: taking damage drains soul, halves nail damage for 5 seconds, and spawns 1-3 Infected Balloons
- Chaos, Chaos: applies a different modifier every 15 seconds
- Temporal Distortion: occasionally warps the player back to where they were 3 seconds ago
- Poor Memory: makes the HUD lie to you (currently doesn't do anything if you have Hiveblood or Joni's Blessing equipped)
- A Fool’s Errand: covers the ground in spikes or spawns enemies from the Colosseum in waves

## Unique Modifiers
- Nailmaster: disables all attacks except for Nail Arts (applies to the Nailmasters)
- The Ephemeral Ordeal: Grey Prince Zote can spit out Zotelings from the Eternal Ordeal (applies to Grey Prince Zote)
- Something Wicked: invincible Grimmkin Nightmares with no contact damage appear in the arena, dash towards the player, and burst into flame pillars (applies to Nightmare King Grimm)
- Pale Watch: spawns two invincible Kingsmoulds with no contact damage on either end of the arena that occasionally throw boomerangs at the player (applies to Pure Vessel)
- Forgotten Light: modifies Absolute Radiance’s attack patterns, guaranteed to appear with High Stress (applies to Absolute Radiance)
  - Eye Beams: light beams last longer and rotate in alternating directions
  - Sword Burst: both swords waves spawn at the same time and move slower with a sharper curve
  - Sword Wall/Sword Rain: accelerate as they move across the arena
  - Orbs: spawn a light beam when they collide with an object or dissipate
  - Climb: light beams have a shorter telegraph and target the player with perfect accuracy

## Adding custom modifiers
To create a Challenge Mode addon, make a new mod project and add a reference to Challenge Mode. Create a new class that extends from the abstract Modifier class, which has the following methods:

- StartEffect: where you instantiate GOs, add hooks, etc.
- StopEffect: where you destroy GOs, unhook, etc.
- ToString: name, in the format of YourAddonName_YourModifierName
- GetCodeBlacklist: a list of the internal names of modifiers that are fundamentally incompatible with this modifier (note: Challenge Mode modifiers are named in the format ChallengeMode_ModifierName)
- GetBalanceBlacklist: a list of the internal names of modifiers that are too difficult/too easy when they appear with this modifier

Your custom modifier must implement StartEffect, StopEffect, and ToString. After creating the modifier, go into the mod class and add the following line to Initialize:
```
ChallengeMode.ChallengeMode.AddModifier<YourModifierHere>();
```
If the modifier is a unique modifier, use the following line instead:
```
ChallengeMode.ChallengeMode.AddModifierU<YourModifierHere>("Name of the scene you want this modifier to appear in");
```
The mod class must also have a load priority above 1 to ensure that Challenge Mode is initialized before the addon:
```
public override int LoadPriority() => int.MaxValue;
```
Finally, build the project and launch Hollow Knight. The custom modifier should now be able to appear in game and be selectable in the mod menu under the "Guarantee modifier" option.
