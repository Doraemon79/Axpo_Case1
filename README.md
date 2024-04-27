# Axpo_Case1
Report generator


CLI start:

in the folder of ReportGeneratorApp type dotnet run and the preferred input example:
dotnet run --interval 2 --timezone "America/New_York"

Time offset is calculated based on the region.
It can execute one day by time.
There is a retry mechanism which makes 5 attempts of extraction before giving an error.

Comments:
Test coverage is 88%  with longer tme I would have made the classes differently to increase the coverage
I still do not understand how the interval is used and when the app should stop.
So if we have an interval the program should run and extract trades every n minutes, so when should it stop?
In the examples given there are days in the past. What is the point? If the date is in the past, power positions could not change, so why not fetch all the data at once?
I really hope semi-column is a typo .

I find the name of the output file inconsistent with the name format used in the input.


