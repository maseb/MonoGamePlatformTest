﻿module MBPlat.Physics

open Microsoft.Xna.Framework
open MBPlat.Actors

let IsStatic (actor : WorldActor) = 
    match actor.BodyType with
    | Static -> true
    | _ -> false

let PartitionWorldObjects worldObjects = 
    worldObjects
        |> List.partition IsStatic

let HandleCollisions (worldObjects : WorldActor list) = 
    let staticObjects, dynamicObjects = PartitionWorldObjects worldObjects

    let FindNewVelocity rect1 rect2 velocity = 
        let inter = Rectangle.Intersect(rect1, rect2)
        let mutable (newVel : Vector2) = velocity
        if inter.Height > inter.Width then
            newVel.X <- 0.f

        if inter.Width > inter.Height then
            newVel.Y <- 0.f
        
        newVel
        
    let FindOptimumCollision (a : WorldActor) (b : WorldActor) = 
        match a.ActorType,b.ActorType with
        | Player(h), Obstacle -> match a.BodyType, b.BodyType with
                                 | Dynamic(s), Static -> { a with BodyType = Dynamic((FindNewVelocity a.DesiredBounds b.CurrentBounds s)); ActorType = Player(Nothing) }
                                 | _ -> a
        | _ -> a

    let rec FigureCollisions (actor : WorldActor) (sortedActors : WorldActor list) = 
        match sortedActors with
        | [] -> actor
        | x :: xs -> let a = if actor.DesiredBounds.Intersects x.DesiredBounds then
                                FindOptimumCollision actor x
                             else
                                actor
                     FigureCollisions a xs

    let rec FixCollisions (toFix : WorldActor list) (alreadyFixed : WorldActor list) = 
        match toFix with
        | [] -> alreadyFixed
        | x :: xs -> let a = FigureCollisions x alreadyFixed
                     FixCollisions xs (a::alreadyFixed)
    
    FixCollisions dynamicObjects staticObjects

let AddFriction (actor : WorldActor) = 
    match actor.BodyType with
    | Dynamic(v) -> let newV = Vector2(v.X * 0.95f, v.Y)
                    { actor with BodyType = Dynamic(newV) }
    | _ -> actor


let AddGravity (gameTime: GameTime) (actor : WorldActor) = 
    let ms = gameTime.ElapsedGameTime.TotalMilliseconds
    let g = ms * 0.01
    match actor.BodyType with
    | Dynamic(s) -> let d = Vector2(s.X, s.Y + (float32 g))
                    { actor with BodyType = Dynamic(d) }
    | _ -> actor

let ResolveVelocities (actor: WorldActor) = 
    match actor.BodyType with
    | Dynamic(s) -> { actor with Position = actor.Position + s}
    | _ -> actor