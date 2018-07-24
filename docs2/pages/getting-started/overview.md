# Overview

## Definition

**Entity Framework Plus** is a library that Improve Entity Framework performance and overcome limitations with **MUST-HAVE** features.

## Features

- Batch Operations
  - [Batch Delete](/batch-delete)
  - [Batch Update](/batch-update)
- LINQ
  - [LINQ Dynamic](/linq-dynamic)
- Query
  - [Query Cache](/query-cache)
  - [Query Deferred](/query-deferred)
  - [Query DbSetFilter](/query-db-set-filter)
  - [Query Filter](/query-filter)
  - [Query Future](/query-future)
  - [Query IncludeFilter](/query-include-filter)
  - [Query IncludeOptimized](/query-include-optimized)
- Audit
  - [Audit](/audit)

## Installing 


## Batch Operations

Batch Operations method allow to perform **UPDATE** or **DELETE** operation directly in the database using a LINQ Query without loading entities in the context.

Everything is executed on the database side to let you get the best performance available.

[Learn more](/tutorial-batch-operations)

## LINQ

**LINQ Dynamic** in Entity Framework is supported through the [Eval-Expression.NET](http://eval-expression.net/) Library.

[Learn more](/tutorial-linq)


## Query
### Query Cache

Query cache is the second level cache for Entity Framework.

The result of the query is returned from the cache. If the query is not cached yet, the query is materialized and cached before being returned.

### Query Deferred

Defer the execution of a query which is normally executed to allow some features like Query Cache and Query Future.

### Query Filter

Filter query with predicate at global, instance or query level.

### Query Future

Query Future allow to reduce database roundtrip by batching multiple queries in the same sql command.

All future query are stored in a pending list. When the first future query require a database roundtrip, all query are resolved in the same sql command instead of making a database roundtrip for every sql command.

### Query IncludeFilter

Entity Framework already support eager loading however the major drawback is you cannot control what will be included. There is no way to load only active item or load only the first 10 comments.

### Query IncludeOptimized

Improve SQL generate by Include and filter child collections at the same times!

[Learn more](/tutorial-query)

## Audit

Allow to easily track changes, exclude/include entity or property and auto save audit entries in the database.

[Learn more](/tutorial-audit)

## Contribute

The best way to contribute is by spreading the word about the library:

 - Blog it
 - Comment it
 - Fork it
 - Star it
 - Share it

A **HUGE THANKS** for your help.
