# 2D PLATFORMER DEMO
A 2D Character Controller Template for Game 340

**'A character controller should CUT AS MANY CORNERS as it needs to in order to execute a player's intention.' - Sun Tzu, the Art of War**

- [This Website](https://maddythorson.medium.com/celeste-forgiveness-31e4a40399f1) explains 50% of this character controller
- [This Youtube Video](https://www.youtube.com/watch?v=yorTG9at90g) explains everything else

# KEY NOTES

## This is a learning tool
- This is just a guideline. I probably made many mistakes, but hopefully looking at this can save you time
- It's more important that you understand the concepts than every little bit about the code
## This is a Rigidbody2D Controller
- I wanted physics based games, but I couldn't find any quality 2D character controllers that were Rigidbody
- If you don't like working with Rigidbodies, [here's a great alternative](https://www.youtube.com/watch?v=3sWTzMsmdx8)!
### Rigidbody Pros:
- Easier to integrate with other mechanics. Pushing boxes, physics effectors, materials, getting pushed, softbodies, etc...
### Rigidbody Cons:
- Less control over movement
- Trusting how Unity moves Rigidbodies [(This is an example of a better collision system that Unity doesn't use)](https://www.youtube.com/watch?v=YR6Q7dUz2uk)

## There will be flaws

- If something doesn't make sense, change it
- Your game will likely require something different from this template.

## This example is a little bloated

This sample includes:
- An Input Manager Package
- Wall clinging, sliding, jumping
- Aseprite animation
- Squash + Stretch
- Tilemaps

# SCRIPTS ARCHITECTURE / OVERVIEW

Player Input -> **Input Manager -> Entity Controller -> Motor -> Motor Modules** -> Character Movement

## Input Manager
- **Separated from the controller:** if you need to control multiple entities at a time, or swapping between them it's as easy as enabling/disabling them
- **Sample Input Actions uses N/E/S/W for controller buttons instead of explicit A/B/X/Y:** consistent controls across all controllers:
- This input manager has a few features
    - [Input Buffering](https://supersmashbros.fandom.com/wiki/Input_Buffering)
    - Binding 'input receivers' to button inputs
    - Gestures, such as **Double Tap** or **Charge and Hold**
    - A priority system, so only the highest priority input receivers are used
    - [More documentation on this](https://github.com/ElliottHood/Input-Management)

## Entity Controller
- Keeps track of general information required for most controllers
    - [State Machine](https://www.reddit.com/r/gamedev/comments/1h4dfud/understanding_state_machines_for_player)
    - Hurtbox + Hitbox
    - Knockback
- Enable/disable different motors

## Motor
- Manages motor modules and contains a reference to the rigidbody
- Separate from the Entity Controller in case a single entity needs multiple motors, or you need a different motor for an AI

## Motor Modules
- **SHOULD NEVER BE REFERENCED SPECIFICALLY BY THE MOTOR**
- Individual scripts for movement mechanics
- If they reference other modules, they should still function **WITHOUT** them
- All modules must be placed on the same component as the Motor and other modules
    - PlatformerMotor uses ```GetComponents<PlatformerMotorModule>()```
    - Modules use ```GetComponent<SpecificOtherModule>``` for their references
    - Avoids serialized references, as that adds overhead to adding/removing movement modules
    - You may want to change this if you want 2 motors on a single controller

# PROJECT SETTINGS

## Gravity
- **Gravity set to -25 instead of -9.81:** Most games use higher gravity settings than real life, because it feels bad to float in the air for a long time after you jump

## Collision Matrix + Layers

```LayerEnumerator``` and ```TagEnumerator``` scripts are included, so you can reference them in code. Ex:
```
gameObject.layer = Layers.Platform;
```

Collision Logic:
- **Separate layer for Platform:** so the controller's ```GroundCheck``` can detect / avoid platforms
- **Entity layer does not collide with itself:** so multiple controllers can move through each other
- **Hitbox and Hurtbox layers only collide with each other:** optimization to prevent unnecessary TryGetComponent() calls as they are expensive

# THE ACTUAL CONTROLLER

## Features + Controls
- I refer again to [this excellent site on Celeste's controller](https://maddythorson.medium.com/celeste-forgiveness-31e4a40399f1)
### Input Buffering
- Storing input for later. If you press the jump button a short time before landing, you will jump on the exact frame that you land.
### Horizontal Movement Module
- Controls horizontal movement, acceleration, drag, and top speed
    - Air and grounded settings
- NOTE: there may be a better way to do this!
    - This is a flaw of Rigidbody2D Movement, as we don't want to ever SET our velocity, since we want to interact in a way that makes sense with other objects
        - We instead opt to add proportional force that will get us to a target speed
    - Because of this it's difficult to configure the exact acceleration times / curves
- I refer again to [this video as to why these settings are important](https://www.youtube.com/watch?v=yorTG9at90g)
### Jump Module
- **Coyote Time**
    - You can still jump for a short time after leaving a ledge.
- **Head Bonk Detection**
    - If you bonk your head on the ceiling, the game will give you a moment of floatiness so it feels less jarring
- **Jump Corner Correction**
    - If you bonk your head on a corner, the game tries to wiggle you to the side around it.
- **Wide Wall-Jump Window**
    - You can actually wall jump 2 pixels from a wall
- Double jumps
    - Reset when grounded
### Wall Module
- Needs a state machine for the different climbing states, as it's more complex. This is a really complicated class!
- **Holding into a wall** will allow you to cling to it
    - Also resets double jumps!
- **Letting go** will have you slide down the wall
- **Holding up** will have you climb the wall
- **Holding down** will have you slide down the wall, but you won't fall off! (separate animation state)
- **Jumping** will wall jump and push you outwards
### Crouch Module
- Purely visual (doesn't actually change the hurtbox / collision)
- Stops horizontal movement when crouched
    - Accomplished in the HorizontalMovementModule detecting if the CrouchModule exists

## Rigidbody
- **Freeze rotation:** Z - our character should stay right side up
- **Interpolate:** gives us smooth movement when visualizing
- **Collision Detection:** Continuous - ensures player doesn't clip through thin objects when moving quickly
- **Gravity:** 1.75 - higher gravity gives us more control over the player controller
- **Material:** Slippery - a material with no friction so we can control drag manually

## Separate the collision and the visuals to Child GameObjects
- Collision
    - The collision and ```GroundCheck``` should be separated in case we need to add/remove colliders
- Sprite
    - **Child of an anchor:** we can change the ```transform.localScale``` of this anchor to squash and stretch the sprite
        - Don't want to deform the collider
    - Animator logic goes here

## Collision
- **Platformer Collider Manager:**
    - A helper script that adds slopes on the bottom of the ground to prevent tilemap collision errors from killing all horizontal momentum
    - Also resizes the groundcheck
- **Ground Check:**
    - Checks for ground every frame. Keeps track of last grounded time for [Coyote Time](https://www.mattmakesgames.com/articles/celeste_and_forgiveness/index.html)
    - A 'no platform' check, that ensures the groundcheck doesn't return positive if the player is jumping up through a platform

# OTHER FEATURES + REMOVING THEM

### Squash and Stretch

- Uses DOTween
- Needs to be on a parent of the player's Sprite Renderer. The pivot is important!
- Animates based on landing, jumping, and crouching
- TO REMOVE: remove the script from any prefabs and delete the script

### Animation

- The animator is simply observing all the modules and motor states and displaying an appropriate animation
- Using the [Aseprite](https://www.aseprite.org) integration package, editing the animations is super easy.
    - The free version [Libresprite](https://libresprite.github.io/#!) works too, but takes a little more work to setup and isn't updated
    - Automatically creates and Animator Component and Animations based on Loop Tags in your animation
        - The states are named the same way, (see ```PlatformerAnimator```)
    - Create animation events using string tags in Aseprite to do things like play footsteps/particles
- THE UNITY ANIMATOR ISN'T YOUR FRIEND FOR SPRITESHEETS/PIXELART ANIMATION.
    - I highly recommend that you [animate through code](https://www.youtube.com/watch?v=ZwLekxsSY3Y) like in the ```PlatformerAnimator```
- TO REMOVE: remove the Aseprite Integration package, and delete this script.
    - Nothing should reference this animator script! It's job is to observe everything and visualize it
    - View layer should never touch the Model layer

### Hitbox / Hurtbox

- Two layers (Hitbox and Hurtbox) are added to this project and are set to only collide with each other
- Hitboxes:
    - Deal damage and knockback
    - Broadcast events when hitting something
        - The ```FirstHit``` event only triggers on the first substantial Hurtbox per activation
- Hurtboxes receive damage and keep track of:
    - Current Health
    - Substantiality (background decorations are not substantial, and don't trigger FirstHit events)
    - Handle Intangibility / Invulnerability
    - Broadcast events when getting hit / dying (health reaches 0)
- TO REMOVE:
    - Delete the Hitbox and Hurtbox Layers
    - Remove knockback/hitstun implementation from FighterController (sorry, this part will be messy)
    - Remove Hitbox/Hurtbox scripts from prefabs
    - Delete ```Plugins/DamageSystem```

### Tilemaps

- In the Sample Scene you'll find tilemaps
- The demo tiles are half size (8x8), as the base PPU for the project is 16
- Separate tilemaps
    - Ground
        - Rendered above the Default Sorting Layer
        - Your typical tilemap!
        - Collision
            - Famously janky in Unity
            - Uses CompositeCollider to help mitigate janky by rubbing against tiles
    - Platform
        - Shifted slightly down to prevent clipping through the floor
        - Uses Platform Effector
        - On Platform Layer
    - Foreground / Background Tiles
        - On their own sorting layers
        - Just exist for decoration
- The tiles adjust their sprites automatically, and you can paint using ```Window > 2D > Tile Palette```
    - This is done with [Unity's Rule Tile](https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@1.6/manual/RuleTile.html)
    - I made these partially using [TileSetter](https://www.youtube.com/watch?v=SI3mZ3ynrTw), check out this demo it's INCREDIBLE
    - Make sure you're **painting on the right tilemap** by selecting it!

### Regule5 Font

- A pixel font asset I converted to work in Unity from [this page](https://pixeloverload.itch.io/basicfonts)
- Didn't have time to increase the size of spaces (forgive me)
- Documentation on how to do it in the Fonts README file, but that tutorial may no longer work in Unity 6.0+

# ADDING NEW STUFF

## New Modules

- NEVER REFERENCE THE MOTOR CLASS
- Reference other modules as little as possible, and use ```GetComponent<OtherModule>``` when doing so
- Remember that the Animator should observe all modules, rather than getting contacted directly

## Effects

- A 'VisualEffect' class is what I use, that can:
    - Screenshake / Screen pull
    - Play a particle / animation at a position
    - Play sound effects
    - Controller rumble
- Damage flashes when getting hit does wonders

## Abilities (and attacks)

A little coding challenge for you. Some notes from my experience doing so:

- **Handle input properly**
    - Try using the ```InputReceiver``` class, and bind it to an input (and always unbind when destroyed!)
    - For a demo project for that, [see here](https://github.com/ElliottHood/Input-Management)
    - Ensure input is user-friendly (try to capture player intention!)
- Create abilities on **separate GameObjects**
- Activating abilities that aren't instant should change the FighterController's state to Ability
    - This stops the motor and gives full control to the ability
- Should be able to add/delete/pause/interrupt all abilities
- Abilities should be paused/interrupted in hitstop

For melee attacks specifically, make sure that the attacker enters HitStop as well!
