# Dynamic Concept 

Tenant Platform (A collection of applications for a collection of users) 
-> Application (A specific application in the collection)

-> Principals
->-> User Principal (An interactive user)
->-> System Principal (A system internal user representing an application itself)
->-> Service Principal (An external principal interacting with the application)

->-> Fields (Templates for types of table column that have pre-modelled features (say decimal, integer, string, Name, PhoneNumber, Email, PostalCode, CurrencyAmount, these should also have html / js / validation rules react components for editor, viewer defined for display - THESE ARE EFFECTIVELY CUSTOM TYPES OF COLUMN AND WITH VALIDATION AND DISPLAY DATA)

->-> Tables (Collections of fields stored and forming the data model of the platform - these features update the skiplist indexes automatically, this will also need an autoNumbered key defined (and partition ID will be tenantid + key))
->->-> Indexes (Implemented with Skip-lists and inverted skip-lists for speedy queries - all fields are indexed)
->->-> Data Views (Tabular Queries across the model - these use the indexes for lookup)

->-> Menus (Sets of menus that define what a user can do in terms of Tasks / Forms / Reports inside an application

->-> Tasks (Background Processes that can be performed inside an application)
->-> Params (Saved Parameter sets that can be applied to schedule tasks in the background or set defaults for forms / reports)
->-> Forms (Screens that can be opened inside an application to view and edit data)
->-> Reports (Tabular layouts that can be opened inside an application to view data)

->-> Role (Each principal can also be assigned roles)
->->-> Permission (Each role can contain permissions which determine what a user can do in each application)

Each of the above tables requires CRUDRQP (Query/Patch) microservices and APIs created in ASP.NET Minimal APIS
The code should only run locally but should support distributed architecture where replication is performed at the log level. 

Let's take this one by one. 

Firstly lets define the Tenant Platform 

We need a list of applications (One should be user management, one should be application definition, one should be an application launcher / interpreter / renderer and three lists of principals 
One of which needs to contain a well-known user created by the system on first boot. 



