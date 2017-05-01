# GameEngineSnippets

This is a repository for sharing a portion of my engine code with other people.

Files that contain the most "structure"

Skills/Skill.cs

Skills/Buff/BuffController.cs

The most basic explanation of this repository in the context of the game engine, is my Objects in Unity are structured such that

```
Player
  Skill[0]
  ...
  Skill[3]

Skill[n]
  BuffController
  Buff[n]
```  
And after a skill applies a buff to a player Skill[n] -hits-> Player

```
Player
  BuffControlller - Copy
  Buff[n]
```

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
