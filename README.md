# 2D PLATFORMER DEMO
A 2D Character Controller Template for Game 340

Inspired by [This Youtube Video](https://www.youtube.com/watch?v=yorTG9at90g), and a lack of good 2D Rigidbody controllers

# KEY NOTES

## This is a Rigidbody2D Controller
- The only quality 2D character controllers I could find
- If you don't like working with Rigidbodies, [here's a great alternative](https://www.youtube.com/watch?v=3sWTzMsmdx8)
### Rigidbody Pros:
- Easier to integrate witih other mechanics. Pushing boxes, physics effectors, materials, getting pushed, softbodies, etc...
### Rigidbody Cons:
- Less control over movement
- Trusting how Unity moves Rigidbodies [(This is an example of a better collision system that Unity doesn't use)](https://www.youtube.com/watch?v=YR6Q7dUz2uk)

## There will be flaws

- If something doesn't make sense, change it
- Your game will require something different from this template

## This example is a little bloated

This sample includes:
- An Input Manager Package
- Wall clinging, sliding, jumping
- Basic animation
- Squash + Stretch
- Tilemaps


# SCRIPTS ARCHITECTURE / OVERVIEW

Player Input -> **Input Manager -> Entity Controller -> Motor -> Motor Modules** -> Character Movement

## Input Manager
- **Separated from the controller:** if you need to control multiple entities at a time, or swapping between them its as easy as enabling/disabling them
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

## Motor Modules
- Individual scripts movement mechanics that aren't tied to the main motor. **These should not reference the motor, and should only *optionally* reference other modules**
- Avoids serialized references, as that adds overhead to adding/removing movement modules (you may want to change this if you want 2 motors on a single controller)

# PROJECT SETTINGS

## Gravity
- **Gravity set to -25 instead of -9.81:** Most games have a higher gravity settings than real life, because it feels bad to float in the air for a long time after you jump

## Collision Matrix + Layers

A Layer and Tag Enumerator script is included, so you can reference them in code:
```
gameObject.layer = Layers.Hitbox;
```

Collision Logic:
- **Separate layer for Platform:** so the controller's GroundCheck can detect / avoid platforms
- **Entity layer does not collide with itself:** multiple controllers move through each other
- **Hitbox and Hurtbox layers only collide with each other:** optimization to prevent unnecessary TryGetComponent() calls


# THE CONTROLLER PREFAB

## Rigidbody
- **Freeze rotation:** Z - our character should stay right side up
- **Interpolate:** gives us smooth movement above 60 fps
- **Collision Detection:** Continuous - ensures player doesn't clip through thin objects when moving quickly
- **Gravity:** 1.75 - higher gravity gives us more control over the player controller

## Motor Scripts Location
- The motor script and all modules are overall logic that should be on the parent of the visuals and sprite 

## Collision

- **Platformer Collider Manager:**
- **Ground Check:**
    - Checks for ground every frame. Keeps track of last grounded time for [Coyote Time](https://www.mattmakesgames.com/articles/celeste_and_forgiveness/index.html)
    - A 'no platform' check, that ensures the groundcheck doesn't return positive if the player is jumping up through a platform

### 


# Other Features (and how to remove them)

### Squash and Stretch

### Animation

### Hitbox / Hurtbox
