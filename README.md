# uwajima
Booking statuses updater

### Description
Service receives a list of needed to refresh statuses bookings IDs from EDO
and then sends them back to EDO

### Infrastructure Dependencies
* Access to Vault

### Project Dependencies
Service must have ability to connect to 
[Identity](https://github.com/happy-travel/odawara) 
and [EDO](https://github.com/happy-travel/edo) services.

### Service lifetime
Service shuts down after doing it's job
