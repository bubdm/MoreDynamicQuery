![query-analysis](https://user-images.githubusercontent.com/8418700/140911412-9fee2581-b96c-4e7b-ba99-a0127321a335.png)

```cs
// Data source
var people = PeopleDataGenerator.GetPeople(2500);

var filters = new List<DynamicFilter>();

var df1 = new DynamicFilter()
{
    ComparisonFilter = ComparisonFilter.DoesNotContain,
    PropertyName = nameof(Person.Name),
    PropertyValue = "h"
};

var df2 = new DynamicFilter()
{
    ComparisonFilter = ComparisonFilter.Contains,
    PropertyName = nameof(Person.FamilyName),
    PropertyValue = "f"
};

filters.Add(df1);
filters.Add(df2);

// var result = people.DynamicWhere(df1, df2).ToList();
var result = people.DynamicWhere(filters).ToList();
```

![MoreDynamicQuery](https://user-images.githubusercontent.com/8418700/141125107-9409724b-9165-4cee-9ef6-3e72f6194bf0.png)

It supports these comparison filters:

```
LessThan
LessThanEqual
GreaterThan
GreaterThanEqual
Equal
NotEqual
IsNullOrEmpty
IsNotNullOrEmpty
IsNullOrWhiteSpace
IsNotNullOrWhiteSpace
Contains
DoesNotContain
StartsWith
DoesNotStartWith
EndsWith
DoesNotEndWith
Like
NotLike
```

<hr/>
<div>Icons made by <a href="https://www.flaticon.com/authors/flat-icons" title="Flat Icons">Flat Icons</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
