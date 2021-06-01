# L2Proxy
A simple MITM Proxy for Lineage 2

Java side link: https://gist.github.com/Elfocrash/644d99e27cfb798220340aadffd684e6

### Features

* Gameserver invisibility option - You are able to hide your server behind the proxies and keep your real server IP secret
* RealIP - Usually with MITM proxies, since the traffic is funnelled from one server, you normally lose the real IP of the player which limits a lot of the functionality that you might have implemented. L2Proxy allows the LoginServer to pass the real IP of the used to the Gameserver during the Login->Gameserver player handoff
* An API - You can use the API in L2Proxy to check the stats of your Proxies, see the active connections to it and even disconnect a specific use IP or blacklist it
* IP Blacklist - You can blacklist a specific IP its connection will be rejected on the proxy level before it ever gets to the gameserver. This includes malicious connections. You can also use the API to blacklist someone and get them instantly disconnected. 
* Multiple Proxies from one app - No real reason for this to exist but I added it anyway

### API Actions

* Get all proxies - `GET` `http://localhost:6969/api/proxies`
* Get proxy by IP and Port - `GET` `http://localhost:6969/api/proxies/127.0.0.1/7778` (IP is the proxy ip and 7778 is the proxy port)
* Disconnect an active connection - `DELETE` `http://localhost:6969/api/proxies/127.0.0.1/7778/127.0.0.1/11571` (First IP is the proxy IP, second ip is the client ip, first port is the proxy port and second port is a client port)
* Get all blacklisted IPs - `GET` `http://localhost:6969/api/blacklist`
* Check if IP is blacklisted - `GET` `http://localhost:6969/api/blacklist/127.0.0.1` (The IP is the user IP)
* Blacklist an IP and disconnect used - `POST` `http://localhost:6969/api/blacklist/127.0.0.1` (The IP is the user IP)
* Removed an IP from the blacklist - `DELETE` `http://localhost:6969/api/blacklist/127.0.0.1` (The IP is the user IP)
