# Dummy
Dummy is a plugin for Unturned / RocketMod 4. It spawns a dummy and shows the amount of damage when damaged.

## Requires libraries

- [Harmony](https://github.com/pardeike/Harmony)


# Commands
- /dummy create - Creates a dummy with unique ID.

- /dummy copy - Creates a dummy with unique ID and copy skin, clothes, beard, hair, face from an owner.

- /dummy remove &lt;id&gt; - Remove dummy by ID.

- /dummy clear - Clear all dummies.

- /dummy execute &lt;id&gt; &lt;command&gt; - Execute a command.

- /dummy teleport &lt;id&gt; - Teleport to you a dummy.

- /dummy gesture &lt;id&gt; &lt;gesture&gt; - Send gesture to a dummy. Gesture can be found [here.](https://github.com/EvolutionPlugins/Dummy#gestures)

- /dummy stance &lt;id&gt; &lt;stance&gt; - Send stance to a dummy. Stance can be found [here.](https://github.com/EvolutionPlugins/Dummy#stances)
        
- /dummy face &lt;id&gt; &lt;faceIndex&gt; - Send face to a dummy. FaceIndex can be found [here.](https://github.com/EvolutionPlugins/Dummy#index-of-faces)

# Permission
- dummy

# Configuration
- AmountDummiesInSameTime _( default: 1 )_ - Max dummies in same time.

- KickDummyAfterSeconds _( default: 300 )_ - Kick automatically dummy after amount of seconds.

# Support OpenMod?
Yes! https://github.com/EvolutionPlugins/Dummy/tree/openmod

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
