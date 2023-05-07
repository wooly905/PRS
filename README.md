[![prs](https://github.com/wooly905/PRS/actions/workflows/build.yml/badge.svg)](https://github.com/wooly905/PRS/actions/workflows/build.yml)
[![NuGet latest version](https://badgen.net/nuget/v/wooly905.prs/latest)](https://nuget.org/packages/wooly905.prs)
[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://lbesson.mit-license.org/)

# PRS
A simple command-line tool to query database schema in Microsoft SQL Server

## Introduction:
This is a tool to help you quickly search the names of objects (columns, tables, stored procedure) in Microsoft SQL server.
This tool has been released as a dotnet global tool. You can install it by the following dotnet command,
>dotnet tool install wooly905.prs -g

The followings are the examples to show you how to use this tool.

The following screenshots show you the tables and columns created by Hangfire.

![pic1](https://user-images.githubusercontent.com/18693681/129477044-0f98c657-97c3-4d98-8540-1f3ee8f4fe2c.JPG)
![pic2](https://user-images.githubusercontent.com/18693681/129477114-e508e069-b4d7-4694-a07f-3eb24f10ad78.JPG)
![pic3](https://user-images.githubusercontent.com/18693681/129477117-53aeb080-3579-4664-8fce-cddeb5f9979c.JPG)

### How to use this tool:

![pic4](https://user-images.githubusercontent.com/18693681/129477875-8d3707f9-c85c-40e4-901e-b6ecdc0cf34a.JPG)

### Setup connection string (prs wcs)

This is the first thing to do for this tool. Make sure your connection string is good with correct host URL, database name, username, and password.

![pic5](https://user-images.githubusercontent.com/18693681/129477397-7de49c08-5844-4938-a73c-bf93377af9c4.JPG)

### Show connection string (prs scs)

![pic6](https://user-images.githubusercontent.com/18693681/129477454-07529fde-7f86-49bc-83fd-573251a509d4.JPG)

### Dump schema to local machine (prs dds)

This is the command to dump the database schema into your local user folder. 
This command must be executed for any query command below.

![pic7](https://user-images.githubusercontent.com/18693681/129477524-83185aa4-871d-47c6-beed-0cf169d168bc.JPG)

### Find table (prs ft) - table name can be *partial*

![pic8](https://user-images.githubusercontent.com/18693681/129477565-0ada3957-a093-4a77-a8c4-bf055ac5677d.JPG)

### Find column (prs fc) - find matched column in all tables

![pic9](https://user-images.githubusercontent.com/18693681/129477624-0573396e-f94e-4217-b3b3-b5f007f3fe7b.JPG)

### Find column in table(s) (prs ftc)

![pic10](https://user-images.githubusercontent.com/18693681/129477674-f2559954-3d2f-43d8-b5c7-269543f2ad0d.JPG)

### Display all columns of a table

![image](https://user-images.githubusercontent.com/18693681/129477743-6f0b787b-94b2-4997-97e0-129d72c9c736.png)

### Find stored procedure

> prs fsp [full or partial name of a storec procedure to find]

