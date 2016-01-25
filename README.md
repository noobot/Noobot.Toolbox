# Noobot.Toolbox
[![Build status](https://ci.appveyor.com/api/projects/status/n9rdym0uo7f1c65m?svg=true)](https://ci.appveyor.com/project/Workshop2/noobot-toolbox) [![Nuget.org](https://img.shields.io/nuget/v/Noobot.Toolbox.svg?style=flat)](https://www.nuget.org/packages/Noobot.Toolbox) [![NuGet](https://img.shields.io/nuget/dt/Noobot.Toolbox.svg)](https://www.nuget.org/packages/Noobot.Toolbox)

A collection of usefull tools and extensions to use with Noobot.Core

## Installation
 
```
Install-Package Noobot.Toolbox
```


## Usage

``` cs
TODO
```

##Features

  - Admin
  -- Simple concept of giving some users (given they know the admin 'pin') extra abilities to remotely manage a noobot instance.
  - Scheduling
  -- Scheduling allows you to simulate a message being received from a person on a regular interview (hourly, daily, nightly) which allows you to run and report on features. E.g. Check the website is up, initiate a relesse to live...

### Middleware
  - AutoResponder
  -- Tests PMing a user (any message received will be returned to a users PM channel)
  - Calculator
  -- Will calculate the answer to maths problems. Can be triggered by both ```/calc 1+1```  or by saying ```1+1```
  - Flickr
  -- Will find a picture about the topic you give it e.g. ```/flickr flowers```
  - Joke
  -- Will tell you a random joke (_warning: these may be NSFW_)
  - Ping
  -- Will send a message via PM every second until stopped
  - Welcome
  -- Will respond with "Hello", a good example of an easy middleware
  - Yield
  -- Should probably be deleted. Proves you can have long running reponders without holding up other incomming messages 

### Plugins
  - Storage
  -- A simple way of storing data/configuration for a given features.
