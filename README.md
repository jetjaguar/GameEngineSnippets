# GameEngineSnippets

This is a repository for sharing a portion of my engine code with other people.

![Editor](http://i.imgur.com/PugOXns.png)

These code snippets represent the "Actions/Verbs" of the Game Engine. Hero 1 "Fireballed" Enemy 1, Hero 3 "Healed" Hero 2, etc...
My "Skills" are broken up into smaller actions/verbs, representing individual steps in completing a skill. Attacking someone with a sword consists of a few steps.
```
Step 1) Walk over to them

Step 2) Swing Sword

Step 3) Return to starting position
```
When applicable, I've condensed highly coupled actions into one action, but in the future it may be required that these actions be split up. An example of this is while "Swording" someone takes 3 steps, I have condensed Steps 1 & 2 into one byte, so from above
```
Step 1) Walk over to them - Melee Attack Byte

Step 2) Swing Sword - Melee Attack Byte

Step 3) Return to starting position - Move to Start Puck Byte
```
Files that contain the most "structure"
```
Skills/Skill.cs

Skills/Byte/SkillByte.cs

Skills/Byte/PlusSkillByte.cs

Skills/Byte/DamagingSkillByte.cs
```
The most basic explanation of this repository in the context of the game engine, is my Objects in Unity are structured such that

```
Player
  Skill[0]
  Skill[1]
  Skill[2]
  Skill[3]

Skill
  Byte[0]
  ...
  Byte[n]

Byte
  BuffController[0]
    Buff[0]
    ...
    Buff[n]
  BuffController[1]
  BuffController[2]  
```  
And after a skillbyte applies a buff to a player, SkillByte[n] -hits-> Player

```
Player
  BuffControlller - Copy
  Buff[0 - n]
```

When I reference Inspector elements/variables with the [SerializeField] tag, I'm talking about the similarly named (and ordered) fields contained in the Script's configuration fields.

This Unity feature allows developers to configure multiple copies with multiple options, and Unity will save those different configurations for use in your game engine.

To help contextualize what this engine code runs, please refer to:

http://imgur.com/a/tkxC3

The main game loop is

1) Equip Heroes with different skills, and start the battle

2) Hero AI decides what skills to use on allies and enemies

3) You tell the Hero AI if that was a good/bad/neither decision

4) Hero AI decides what to do with that info

5) Monsters drop new SKills to give to your heroes

6) Exit battle and return to Step 1

All images in the gifs are placeholder, the Final Fantasy series is a big inspiration for the development of the game,
it's assets are temporary art until we can get some original art
