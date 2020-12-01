﻿open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.EndpointRouting

let handler1 : HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        ctx.WriteTextAsync "Hello World"

let handler2 (firstName : string, age : int) : HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        sprintf "Hello %s, you are %i years old." firstName age
        |> ctx.WriteTextAsync

let handler3 (a : string, b : string, c : string, d : int) : HttpHandler =
    fun (_ : HttpFunc) (ctx : HttpContext) ->
        sprintf "Hello %s %s %s %i" a b c d
        |> ctx.WriteTextAsync

let endpoints =
    [
        GET [
            route  "/" (text "Hello World")
            routef "/%s/%i" handler2
            routef "/%s/%s/%s/%i" handler3
        ]
        GET_HEAD [
            route "/foo" (text "Bar")
            route "/x"   (text "y")
            route "/abc" (text "def")
        ]
        // Not specifying a http verb means it will listen to all verbs
        subRoute "/sub" [
            route "/test" handler1
        ]
    ]

let notFoundMiddleware =
    "Not Found"
    |> text
    |> RequestErrors.notFound
    |> GiraffeMiddleware.create

let configureApp (appBuilder : IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
        .Use(notFoundMiddleware)
    |> ignore

let configureServices (services : IServiceCollection) =
    services
        .AddRouting()
        .AddGiraffe()
    |> ignore

[<EntryPoint>]
let main args =
    WebHost
        .CreateDefaultBuilder(args)
        .UseKestrel()
        .Configure(configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0
