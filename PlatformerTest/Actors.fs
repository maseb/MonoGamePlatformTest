﻿module MBPlat.Actors

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

type BodyType = 
    | Static
    | Dynamic of Vector2

type PlayerState = 
    | Nothing
    | Jumping

type ActorType = 
    | Player of PlayerState
    | Obstacle

type WorldActor = 
    {
        ActorType: ActorType;
        Position: Vector2;
        Size: Vector2;
        Texture: Texture2D option;
        BodyType : BodyType
    }

    member this.CurrentBounds
        with get () = Rectangle((int this.Position.X), (int this.Position.Y), (int this.Size.X), (int this.Size.Y))

    member this.DesiredBounds
        with get () : Rectangle = let desiredPos = match this.BodyType with
                                                   | Dynamic(s) -> this.Position + s
                                                   | _ -> this.Position
                                  Rectangle((int desiredPos.X), (int desiredPos.Y), (int this.Size.X), (int this.Size.Y))
                            

let CreateActor (content: ContentManager) (textureName, actorType, position, size, isStatic) = 
    let tex = if not (System.String.IsNullOrEmpty textureName) then
                Some(content.Load textureName)
              else
                None

    let bt = if isStatic then
                Static
             else
                Dynamic(Vector2(0.f, 0.f))

    { ActorType = actorType; Position = position; Size = size; Texture = tex; BodyType = bt; }
