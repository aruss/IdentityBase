# IdentityBase.EntityFramework.zDbInitializer

Data base initializer. This library will check if tables a present in the
database and it will run the migration scripts of a used data access plugin.

If configured it will also seed example data. 

### Note!

The prefix `z` in the name `zDbInitializer` is a hack to load the plugin after
all the data access plugins, check that your plugin does not starts with
`IdentityBase.EntityFramework.zz` This will be obsolete as soon plugin meta
data will be introduced.

### Used libraries

- EntityFramework https://docs.microsoft.com/en-us/ef/