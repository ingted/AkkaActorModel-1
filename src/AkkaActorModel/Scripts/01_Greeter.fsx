﻿#if INTERACTIVE
#r "../../../packages/Akka/lib/netstandard1.6/Akka.dll"
#r "../../../packages/Akka.FSharp/lib/netstandard2.0/Akka.FSharp.dll"
#r "../../../packages/Akka.Remote/lib/netstandard1.6/Akka.Remote.dll"
#endif


open Akka.FSharp
open Akka.Configuration
open System

type SomeActorMessages =
    | Greet of string
    | Hi


let system = System.create "FSharpActors" <| ConfigurationFactory.Default()

let actor =
    spawn system "MyActor"
    <| fun mailbox ->
        let rec again name =
            actor {   // Actor compuatation expression
                let! message = mailbox.Receive()

                match message with
                | Greet(n) when n = name ->
                    printfn "Hello again, %s" name
                    return! again name
                | Greet(n) ->
                    printfn "Hello %s" n
                    return! again n
                | Hi ->
                    printfn "Hello from Again %s State" name
                    return! loop() }
        and loop() =
            actor {
                let! message = mailbox.Receive()
                match message with
                | Greet(name) ->
                    printfn "Hello %s" name
                    return! again name
                | Hi ->
                    printfn "Hello from Loop() State"
                    return! loop() }
        loop()

// .Tell in F# API is <! <?
actor <! Greet "Ricky"
actor <! Hi
actor <! Greet "Ricky"
actor <! Hi
actor <! Greet "Ryan"
actor <! Hi
actor <! Greet "Ryan"
