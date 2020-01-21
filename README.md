# What Listrr does
Listrr creates lists for shows and movies based on your filters. The created lists get updated every 24 hours based on your filters, so Listrr will add all new items that match your filters, and will also remove all items that do not match your filter configuration anymore.

# This fork?

Specifically its an untouched fork from before Ultimate decided to be greedy and require you to pay him to gain features. Not even new features, just locked current ones randomly behind a paywall. Well, this fork still works if you self host! At some point I'll probably figure out how the new one works, but he decided to fully remove appsettings.json and edit/change up its format. So, that will require a bit of work to decipher... What a surprise, hes not happy with $60 a month from git donations that he makes right here and now and still wants to ensure you can't self host and get more money!

Pre) You'll need to either edit a ton or just go make a brand new domain (dot.tk?) and have this be the base site on the root /

1) Clone this git to your box

2) Make a trakt app, redirect uri of https://domain.com/signin-trakt on one line and urn:ietf:wg:oauth:2.0:oob on another

3) Edit appsettings.json

4) If you dont have a SQL server running, build it with 
docker run --network="bridge" -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest

5) In the root listrr folder, docker build --tag cinnalistrr .

6) Figure out the proxying nginx/etc yourself.

7) docker run --network="bridge" -d -it -p port_you_want:80 --network="bridge" cinnalistrr

8) docker logs whatever_hash_the_above_gave_you and hopefully its working and you can browse to the site and continue to see no errors when doing this command

9) Fix whatever issues you have if its not working, dont put in an issue here I won't help. This gave you the base gameplan thats it.

10) You are up. Was that so fucking hard? Its almost like 10 quick bullet points can get anyone up and running, but without those it takes you hours to scour through code and figure out how the entire damn thing works just to be abble to do this. Fuck, I had to capture the network traffic of listrr.pros site just to figure out the trakt redirects, since /signin-trakt doesn't appear anyone in the code.

# What can I use this for ?

## Traktarr
You can create lists with listrr and add them to your traktarr configuration. Traktarr automatically adds the items to your *arr instances.

## Python-PlexLibrary
You can create libraries from your the trakt lists you created with Listrr. As your lists get updated every 24 hours, so does your libraries
