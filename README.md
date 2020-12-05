# Dummy
Dummy is a plugin for Unturned / OpenMod. It spawns a dummy and shows the amount of damage when damaged. Also, very helpful with debugging plugins.

[![Nuget](https://img.shields.io/nuget/v/EvolutionPlugins.Dummy)](https://www.nuget.org/packages/EvolutionPlugins.Dummy/)
[![Nuget](https://img.shields.io/nuget/dt/EvolutionPlugins.Dummy?label=nuget%20downloads)](https://www.nuget.org/packages/EvolutionPlugins.Dummy/)
[![Discord](https://img.shields.io/discord/764502843906064434?label=Discord%20chat)](https://discord.gg/5MT2yke)

# Commands
_Maybe outdated so check help.md to get all commands_
- /dummy create - Creates a dummy with unique ID.

- /dummy copy - Creates a dummy with unique ID and copy skin, clothes, beard, hair, face from an owner.

- /dummy remove &lt;id&gt; - Remove dummy by ID.

- /dummy clear - Clear all dummies.

- /dummy execute &lt;id&gt; &lt;command&gt; - Execute a command.

- /dummy tphere &lt;id&gt; - Teleport to you a dummy.

- /dummy gesture &lt;id&gt; &lt;gesture&gt; - Send gesture to a dummy. Gesture can be found [here.](https://github.com/EvolutionPlugins/Dummy#gestures)

- /dummy stance &lt;id&gt; &lt;stance&gt; - Send stance to a dummy. Stance can be found [here.](https://github.com/EvolutionPlugins/Dummy#stances)
        
- /dummy face &lt;id&gt; &lt;faceIndex&gt; - Send face to a dummy. FaceIndex can be found [here.](https://github.com/EvolutionPlugins/Dummy#index-of-faces)

- /dummy button &lt;id&gt; &lt;buttonName&gt; - Click to button.

- /dummy inputfield &lt;id&gt; &lt;inputFieldName&gt; &lt;Text&gt; - Input text in *InputField*

# Permissions
_Maybe outdated so check help.md to get all permissions
  - EvolutionPlugins.Dummy:commands.dummy
  - EvolutionPlugins.Dummy:commands.dummy.clear
  - EvolutionPlugins.Dummy:commands.dummy.copy
  - EvolutionPlugins.Dummy:commands.dummy.create 
  - EvolutionPlugins.Dummy:commands.dummy.remove
  - EvolutionPlugins.Dummy:commands.dummy.tphere
  - EvolutionPlugins.Dummy:commands.dummy.button
  - EvolutionPlugins.Dummy:commands.dummy.execute
  - EvolutionPlugins.Dummy:commands.dummy.face
  - EvolutionPlugins.Dummy:commands.dummy.gesture
  - EvolutionPlugins.Dummy:commands.dummy.inputfield
  - EvolutionPlugins.Dummy:commands.dummy.jump
  - EvolutionPlugins.Dummy:commands.dummy.stance

# Configuration
_Outdated_
- amountDummies _( default: 1 )_ - Max dummies in same time.

- kickDummyAfterSeconds _( default: 300 )_ - Kick automatically dummy after amount of seconds.

- isAdmin _(default: false)_ - On spawning make a dummy admin

# Tips

## Gestures

|      Gesture     	| Index 	|
|:---------------:	|:-----:	|
| INVENTORY_START 	|   1   	|
|  INVENTORY_STOP 	|   2   	|
|      PICKUP     	|   3   	|
|    PUNCH_LEFT   	|   4   	|
|   PUNCH_RIGHT   	|   5   	|
| SURRENDER_START 	|   6   	|
|  SURRENDER_STOP 	|   7   	|
|      POINT      	|   8   	|
|       WAVE      	|   9   	|
|      SALUTE     	|   10  	|
|   ARREST_START  	|   11  	|
|   ARREST_STOP   	|   12  	|
|    REST_START   	|   13  	|
|    REST_STOP    	|   14  	|
|     FACEPALM    	|   15  	|

## Stances

|  Stance 	| Index 	|
|:-------:	|:-----:	|
|  CLIMB  	|   0   	|
|   SWIM  	|   1   	|
|  SPRINT 	|   2   	|
|  STAND  	|   3   	|
|  CROUCH 	|   4   	|
|  PRONE  	|   5   	|
| DRIVING 	|   6   	|
| SITTING 	|   7   	|

## Index of faces

![face](https://i.redd.it/eqve40c3cuqx.png)

## Index of beards/hairs

![image1](https://i.redd.it/t9qh0q76l16z.jpg)

# Where I can get support
You can get support in my [discord server](https://discord.gg/5MT2yke)
