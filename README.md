# Gradera.ObjectChangeFilter
Mvc filter for checking objects if they have been changed by someone else since you loaded it, this is so we don't save 2 times and override eachothers data.


Usage: 
On save functions
[ObjectChangeFilter(IdentifierProperty = "Id", ChangeType = ChangeType.Save)]

On the get function
[ObjectChangeFilter(IdentifierProperty = "Id", ChangeType = ChangeType.Get)]
