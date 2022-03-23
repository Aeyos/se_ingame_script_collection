// ** START OF SE CODE ** //
/*

    /*~/                                                     \~*\
/*~/    ALight - Aeyos' Lighting Script    \~*\
   /*~/                                                       \~*\

    V E R S I O N
    - - - - - - - - - - - - - - - - - - - - - - - - - 
        1.3.0

    
    A B O U T
    - - - - - - - - - - - - - - - - - - - - - - - - - 
            A script to help you decorate your ship and add dynamic effects to it.
        Lighting is the soul of a space ship, statition, whatever. Take control of
        your lights and create a true lightshow!


    C H A N G E L O G
    - - - - - - - - - - - - - - - - - - - - - - - - - 
        v 1.3.0
        - - - - - - - - - - - - - - - - - - - - - - - - - 
            1.  Added "Pulse" lighting mode (single light with configurable trail
            that runs along the length of lights)
            2. Re-introduced multiple groups (MyGroupName 1, MyGroupName 2, etc.)
            3. Added Commands to control script configuration externally, via timers
            other scripts and even from hotbar
            4. Added commands documentation to custom data section (no need to lookup
            a random text outside of the game)
            5. Added multiple commands argument (separate every command by adding a
            ";" between them. Ex: SET:LIGHT_MODE:GRADIENT;SET:UPDATE_EVERY:10
            6. Renamed mod to Aeyos' Lighting Script to better reflect it's current
            purpose
            7. Updated default configuration

        v 1.2.0
        - - - - - - - - - - - - - - - - - - - - - - - - - 
            1.  Added "Random" lighting mode (chooses a color between those predefined)
            2. Skipping broken or off lights (improves performance

        v 1.1.0
        - - - - - - - - - - - - - - - - - - - - - - - - - 
            1.  Added configuration via the Custom Data field
            2. Added flow direction (First to last or Last to first block in group)
            3. Minified code to prevent confusion
            4. Added HEADER info to this script


    T O D O
    - - - - - - - - - - - - - - - - - - - - - - - - - 
        [ X ] Add offset to color (gradient)
        [ X ] Make configuration a little bit better (from custom data)
        [ X ] Add Random Mode
        [ X ] Improve performance by skipping off / broken lights
        [ X ] Add hotbar commands to control groups
               [    ] Pause/Unpause
               [ X ] Profile configuration (via commands)
               [ X ] Faster/Slower
               [ X ] Change configurations via script/timers/hotbar

    ~ E N J O Y ~

*/
// ** END OF SE CODE ** //