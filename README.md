# TheUltimatePHPArrayConventerConsoleEdition
Just a console program, what can convert CSV table into a PHP array.

# How to use?
1. Get or create a CSV table into a programm directory.
2. Ensure that the table conforms to the syntax required for conversion (for more details, see "Syntax").
3. Launch "TheUltimatePHPArrayConventer.exe".
4. Enter table name.
5. Profit.

# Syntax
## example.csv
### As a table:
| id  | name | items |
| ------------- | ------------- | ------------- |
| 1 | Array 1 | "Item 1\|Item 2" |
| 2 | Array 2 | "Item 1\|Item 2\|Item 3" |
| 3 | Array 3 | "Item 2\|Item 3" |
### As a text:
```
id;name;items
1;Array 1;"Item 1|Item 2"
2;Array 2;"Item 1|Item 2|Item 3"
3;Array 3;"Item 2|Item 3"
```
### Where:
```
';' - array element separator;
'" "' - elements of a nested array;
'|' - nested array element separator;
```

### That’s all for now.
