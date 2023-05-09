# geneva-television

To edit it, open `geneva-television.sln` in Visual Studio (or an equivalent).

To build it, run `build.cmd`. To run it, run the following commands to make a symbolic link in your server data directory:

```dos
cd /d [PATH TO THIS RESOURCE]
mklink /d X:\cfx-server-data\resources\[local]\geneva-television dist
```

Afterwards, you can use `ensure geneva-television` in your server.cfg or server console to start the resource.

NOTE: This resource really isn't meant to be used on a live server... at all. As it does nothing to sync anything. But, if someone would like to use any of this code to make a full resource made to be used on live servers -- i'm okay with that. This is nothing more than a cool remake & concept.
cont. This resource currently works with five (5) TV's around the map. Those are the TV's in Trevor's/Floyd's apt., TV in Jimmy's room, TV in Trevor's trailer, TV in Franklin's vinewood safehouse, TV in Franklin's Aunt's house.

## Features
* Automatically turn off TV if you leave the housing interior of the TV.
* Very close to a 1:1 replica of television from vanilla GTA5.
* It's just cool.

### Planned
* Not sure.

## Preview
[Youtube](https://youtu.be/u1DQNll3G9k)