# cworks.dbtest
Isolated, Repeatable contract and unit testing for your SQL Server DB objects from your .NET Core application.

Testing is important. We test our code to ensure its contact and behavior.  We write unit tests that are highly performant, isolated, and repeatable. 

But what about our database objects?  If our application uses SQL views, stored procedures or functions to perform business logic, shouldn't we be testing those too?

Testing Database object can be challenging for a number of reasons.  The shared nature of the data in the database makes running repeatable, isolated tests difficult. 

Until  now. 

Using DbTest, developers can author tests to ensure the functionality of their stored procedures, functions, views and table as easy as writing unit tests for their code based services. 

DbTests supports .net core applications that leverage Entity Framework, or straight ADO, or a combination of both. DbTests utilizes XUnit for a test runner and is easy to get set up. 

Add a reference to the appropriate cWorks.DbTest package (EF core users should use [cworks.DbTest.EFCore](https://www.nuget.org/packages/cworks.DbTest.EFCore/) , and ADO users should use [cworks.DbTest.ADO](https://www.nuget.org/packages/cworks.DbTest.ADO/)). 

Configure the test runner db connection, and begin authoring your tests. 

Check out the [Wiki](https://github.com/cdarrigo/cworks.dbtest/wiki) for details on getting started and start testing your database object today. 


