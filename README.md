# SqlSprinkles

SqlSprinkles helps you with creating database tests in .NET that rely heavily on SQL queries. 
Database tests are difficult to create and maintain. This library allows you to create SQL files that can be run
and debugged as single file, while they can be parameterized for all your test variations.
Note: this library should only be used in test projects and is not ment for live production code.

# Example

This a simple example to get the idea of the library. We use .sql files to populate or extract data from the database, like:

    /* file: GetUser.sql */
    DECLARE @User VARCHAR(30)
    SET @User = 'John Doe'
    
    SELECT email 
    FROM USERS 
    WHERE user = @User

This will cover the standard case in our tests. Having this code in a file instead of a string in our code have several advantages: 
This file can be stored in our repository and the SQL code analysis will guarantee that the query is syntactical correct. 
Further more, we can easily just execute this file as a query on our database and see an example of the output.

## Parameter substitution
But using this default data is not always the standard case. We want to test what happens when the user is `Jane Doe`, NULL 
or something else. That is where SqlSprinkles will help.

First step is to create a dictionary with all parameters you want to replace and create a new `ParameterManipulator`
with this dictionary collection:
   
   var repDict = new Dictionary<string, string>() {{ "@User", "Jane Doe" }};
   var paraManipulator = new ParameterManipulator(repDict);

Now get the Sql template

    string sqlText = System.IO.File.ReadAllText(@"GetUser.sql");
    
And use it in the parameterManipulator to manipulate the parameters:

    string result = paraManipulator.Replace(sqlText);
    
The result object will contain the following text:

    /* file: GetUser.sql */
    DECLARE @User VARCHAR(30)
    SET @User = 'Jane Doe' -- 'John Doe' was replaced by 'Jane Doe'
    
    SELECT email 
    FROM USERS 
    WHERE user = @User

And using the query will get the password for Jane Doe instead of John Doe.
