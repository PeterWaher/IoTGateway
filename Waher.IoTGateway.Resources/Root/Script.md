Title: Script
Description: Script syntax reference, as understood by {{Waher.IoTGateway.Gateway.ApplicationName}}.
Date: 2016-02-26
Author: Peter Waher
Master: /Master.md
JavaScript: /Events.js

Script syntax reference
=============================

**{{Waher.IoTGateway.Gateway.ApplicationName}}** contains a powerful script parser and evaluation engine. The script engine is not ECMA-compliant. 
Instead, its focus is to provide an efficient and compact script language using mathemathical notation. Following is a brief overview of the syntax 
different script elements. You can also use the [Script Prompt](Prompt.md) to experiment with script syntax.

![Table of Contents](ToC)

=========================================================================================================================================================

Primitive data types
------------------------

Apart from different ways to create compound data types, the following sections provide a short overview of primitive data types available in script.

### Double-valued numbers

Double valued numbers are written using the following syntax (as a regular expression):

	[+-]?[0-9]*([.][0-9]+)?([eE][+-]?[0-9]+)?
	
Examples:

	1
	3.1415927
	1.23e-3

### Complex numbers

Complex valued numbers can be written either by enclosing its real and imaginary parts in a list between parenthesis, as follows:

	(Re, Im)

If no variable named `i` is defined, it is also possible to use the imaginary unit constant `i`, as follows:

	Re+Im*i

Both `Re` and `Im` above are double-valued numbers.

### Boolean values

Boolean values are either written as `true` or `false`.

### Strings

String values are written between single quotes (`'`) or double quotes (`"`). The backslash character (`\`) can be used to escape quote characters, or
special control characters in strings, accordig to the following table.

| Sequence | Meaning |
|:--------:|---------|
| `\'`     | ' | 
| `\"`     | " | 
| `\\`     | \ | 
| `\n`     | New-line character. | 
| `\r`     | Carriage return character. | 
| `\t`     | Tab character. | 
| `\b`     | Backspace character. | 
| `\f`     | Form-feed character. | 
| `\a`     | Audible bell character. | 
| `\v`     | Vertical tab character. | 
| `\xHH`   | Inlude a hexadecimally encoded byte. | 

### Big Integers

Big Integers can be provided by prefixing them with the `#` character. You can optionally provide
a numeric base for the number, by specifying `d` for decimal, `x` for hexadecimal, `o` for octal
and `b` for binary, between the `#` sign and the sign or digits. If no base is provided, decimal base is
assumed. A `+` or `-` sign can be optionally placed after the `#`.

	#[+-]?((d?[0-9]+)|(x[0-9a-fA-F]+)|(o[0-7]+)|(b[0-1]+))
	
Examples:

	#908340580348630802345239423850823402938409234
	#x123ac40958023bc9890098ef098a098094
	#-o123476253765213476523746512736512735
	#d234324238402983409810483057239057091250750
	#b0110101001001111010101010111101010101010
	#-908340580348630802345239423850823402938409234*#x123ac40958023bc9890098ef098a098094
	#o123476253765213476523746512736512735^100

### Rational Numbers

Rational numbers can be provided by dividing one big integer with another.

Example:

	#908340580348630802345239423850823402938409234/#x123ac40958023bc9890098ef098a098094

### Null

The **null** object reference value is written `null`.

=========================================================================================================================================================

Variable references, Constants and Namespaces
-----------------------------------------------

Constants are pluggable into the script engine, and new constants can be defined in any library, by creating classes that implement the 
`Waher.Script.Model.IConstant` interface. Constants and root namespaces are referenced in script by simply writing their names, just as 
variable references are. The [order of presedence][] among them is as follows:

1. If a variable is found having the referenced name, it is returned, regardless if there exists a constant or a root namespace with the same name.
2. If a constant is found having the referenced name, it is returned if there's no variable with the same name, regardless if there exists a root namespaces with the same name.
3. If a root namespace is found having the referenced name, it is returned if there is no variable or constant with the same name.

The following table lists constants recognized by `Waher.Script`. For lists of constants published by other libraries, see the documentation for the
corresponding libraries.

| Constant               | Description                                               |
|:----------------------:|-----------------------------------------------------------|
| `e`                    | Euler's number                                            |
| `π`, `pi`              | Pi                                                        |
| `ε`, `eps`, `epsilon`  | Smallest positive double value that is greater than zero. |
| `∞`, `inf`, `infinity` | Positive infinity.                                        |
| `i`                    | Imaginary unit                                            |
| `C`                    | Set of complex numbers                                    |
| `R`                    | Set of real numbers                                       |
| `Z`                    | Set of integer numbers                                    |
| `Q`                    | Set of rational numbers                                   |
| `∅`, `EmptySet`        | The empty set                                             |

There are also a set of predefined variables:

| Variable               | Description                                                 |
|:----------------------:|-------------------------------------------------------------|
| `Exception`            | Access to current exception object, in a `CATCH` statement. |
| `Now`                  | Current date and time, local time coordinates.              |
| `NowUtc`               | Current date and time, UTC coordinates.                     |
| `Time`                 | Current time, local time coordinates.                       |
| `TimeUtc`              | Current time, UTC coordinates.                              |
| `Today`                | Current date, local time coordinates.                       |
| `TodayUtc`             | Current date, UTC coordinates.                              |
| `Variables`            | Reference to the current set of variables.                  |

**Note**: Names are case sensitive. `r` and `R` point to different objects.

=========================================================================================================================================================

Canonical extensions
-------------------------

The script engine has a feature that automatically generates canonical extensions to operators and functions if the operands and arguments
are vectors, sets or matrices, and the original operator or function is defined for scalar operands and arguments. If an operator or function
expects vector arguments, and matrices are used, the canonical extension sees a matrix as a vector of row vectors.

Example:

	sin([10,20,30]);
	[[1,2],[3,4]]+x

The above is evaluated as:

	[sin(10),sin(20),sin(30)];	
	[[1+x,2+x],[3+x,4+x]]

Etc.

=========================================================================================================================================================

Operators
--------------

The following operators are supported. They are listed in [order of presedence][].

### Encapsulation

#### Parenthesis

Normal parenthesis `(` and `)` can be used to encapsulate operations that have lower [order of presedence][] than surrounding operators, and thus 
control the execution flow. Example:

	a * (b + c)

#### Vectors

Vectors can be explicitly created by listing their elements between square brackets `[` and `]`. Example:

	v:=[1,2,3];

##### Loop construction of vector

Vectors can also be created using any of the `DO`-`WHILE`, `WHILE`-`DO`, `FOR`-`TO`\[-`STEP`\]\[-`DO`\] or `FOR EACH`/`FOREACH`-`IN`\[-`DO`\] 
statements between braces. Examples:

	v:=[DO x++ WHILE X<10];
	v:=[WHILE x<10 : x++];
	v:=[FOR x:=1 TO 20 STEP 3 : x];
	v:=[FOREACH x IN 1..10|0.1 : x^2];
	v:=[FOR EACH x IN 1..10|0.1 : x^2];

**Note**: `DO` can be exchanged with `:`, or completely omitted, except in the `DO`-`WHILE` case.

**Note 2**: You can use the `Break([x])` function to break out of a loop, and the `Continue([x])` 
function to skip to the next iteration. If a value is provided to the `Break` function, the value 
will be considered the last value of the loop. If a value is provided to the `Continue` function,
the value will be considered the value of the current iteration of the loop. If no value is 
provided, the iteration will not produce a value.

**Note 3**: There is a performance penalty of using `Break` and `Continue`, compared to using
conditional statements within the loop. Internally, `Continue` and `Break` use exceptions to exit
from evaluation in the parsed syntax tree. The performance penalty can be as large as 100 times,
and should only be used if a conditional statement is not possible, or too complex to create.

##### Implicit vector notation

Vectors can also be defined implicitly using an implicit vector notation:

	[Expression[ in Elements]:Condition1[,Condition2[,...[ConditionN]]]]

This allows you to define vectors based on the contents of other vectors. Example:

	X:=1..10;
	P:=[x^2:x in X]

##### Selecting elements

Smaller vectors can be created from larger vectors, by allowing the expression in the implicit vector definition to be a simple variable reference 
representing an element in the larger vector, and then allowing the conditions in the definition to limit the elements belonging to the shorter
vector. Example:

	v:=1..100;
	[x in v:floor(sqrt(x))^2=x]

#### Matrices

Matrices can be explicitly created by listing their row vectors between square brackets `[` and `]`. Example:

	M:=[[1,0,0],[0,1,0],[0,0,1]];

##### Loop construction of matrix

Matrices can also be created using any of the `DO`-`WHILE`, `WHILE`-`DO`, `FOR`-`TO`\[-`STEP`\]\[-`DO`\] or `FOR EACH`/`FOREACH`-`IN`\[-`DO`\] 
statements between braces. Examples:

	M:=[DO [x++,x++,x++] WHILE X<10];
	M:=[WHILE x<10 : [x++,x++,x++]];
	M:=[FOR y:=1 TO 20 : [FOR x:=1 TO 20 : x=y ? 1 : 0]];
	M:=[FOREACH x IN 1..10|0.1 : [x^2,x^3,x^4]];
	M:=[FOR EACH x IN 1..10|0.1 : [x^2,x^3,x^4]];

**Note**: `DO` can be exchanged with `:`, or completely omitted, except in the `DO`-`WHILE` case.

**Note 2**: You can use the `Break([x])` function to break out of a loop, and the `Continue([x])` 
function to skip to the next iteration. If a value is provided to the `Break` function, the value 
will be considered the last row of the loop. If a value is provided to the `Continue` function,
the value will be considered the row of the current iteration of the loop. If no value is provided, 
the iteration will not produce a row.

**Note 3**: There is a performance penalty of using `Break` and `Continue`, compared to using
conditional statements within the loop. Internally, `Continue` and `Break` use exceptions to exit
from evaluation in the parsed syntax tree. The performance penalty can be as large as 100 times,
and should only be used if a conditional statement is not possible, or too complex to create.

##### Implicit matrix notation

Matrices can also be defined implicitly using implicit vector notation, where the `Expression` evaluates to row vectors. Example:

	M:=Identity(5);
	[Reverse(Row):Row in M]

#### Sets

Sets are unordered collections of items that can be used in script. There are numerous ways to define sets.

##### Explicit definition of sets

Sets can be explicitly created by listing their elements between braces `{` and `}`. Example:

	S:={1,2,3};

##### Loop construction of set

Sets can also be created using any of the `DO`-`WHILE`, `WHILE`-`DO`, `FOR`-`TO`\[-`STEP`\]\[-`DO`\] or `FOR EACH`/`FOREACH`-`IN`\[-`DO`\] 
statements between braces. Examples:

	S:={DO x++ WHILE X<10};
	S:={WHILE x<10 : x++};
	S:={FOR x:=1 TO 20 STEP 3 : x};
	S:={FOREACH x IN 1..10|0.1 : x^2};
	S:={FOR EACH x IN 1..10|0.1 : x^2};

**Note**: `DO` can be exchanged with `:`, or completely omitted, except in the `DO`-`WHILE` case.

**Note 2**: You can use the `Break([x])` function to break out of a loop, and the `Continue([x])` 
function to skip to the next iteration. If a value is provided to the `Break` function, the value 
will be considered the last value of the loop. If a value is provided to the `Continue` function,
the value will be considered the value of the current iteration of the loop. If no value is 
provided, the iteration will not produce a value.

**Note 3**: There is a performance penalty of using `Break` and `Continue`, compared to using
conditional statements within the loop. Internally, `Continue` and `Break` use exceptions to exit
from evaluation in the parsed syntax tree. The performance penalty can be as large as 100 times,
and should only be used if a conditional statement is not possible, or too complex to create.

##### Implicit set notation

Sets can also be defined implicitly using implicit set notation:

	{Expression[ in Superset]:Condition1[,Condition2[,...[ConditionN]]]}

This allows you to define infinite sets. Examples:

	S:={[a,b]: a>b}
	S:={[a,b]: a in Z, b in Z, a>b}
	S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104}
	S:={x::x>10}
	S:={v[]:count(v)>3}
	S:={s{}:count(s)>3}
	S:={M[,]:M.Columns>M.Rows}

**Note**: To differentiate between creating an [object ex nihilo](#objectExNihilo), and creating a subset, when no superset is defined,
two consequtive colons (`:`) can be used. `{x::x>10}` creates a set of all items that are comparable to `10` and are greater.
`{x:x>10}` creates an [object ex nihilo](#objectExNihilo) with one member `x` that will have a boolean value corresponding to the
greater-than comparison of the variable `x` with `10`.

##### Subsets

Subsets can be created by allowing the expression in the implicit set definition to be a simple variable reference belonging to a superset, 
and then allowing the conditions in the definition to limit the elements belonging to the subset. Examples:

	S:={x in Z:x>10}
	S2:={x in S:x<20}

#### Object Ex nihilo

Objects can be created from nothing, by listing the members between braces `{` and `}`, separating each one with a comma `,` and each member name from its
corresponding value with a colon `:`. Example:

	{
		Member1: Value1,
		Member2: Value2,
		...
		MemberN: ValueN
	}

Member names can be both standard variable references, or constant strings. The following example gives the same result as the example above:

	{
		"Member1": Value1,
		"Member2": Value2,
		...
		"MemberN": ValueN
	}

You can also choose to create an empty object, and assign properties dynamically. Example:

	Obj:={};
	Obj.["a","b","c"]:=1

##### Object methods

You can create objects with methods using object ex-nihilo notation and lambda expression. Example:

```
Obj:=
{
	"Sum":(x,y)->x+y,
	"Prod":(x,y)->x*y,
	"Pow":(x,y)->x^y
};

[Obj.Sum(3,4), Obj.Prod(3,4), Obj.Pow(3,4)]
```

Would return:

```
[7, 12, 81]
```

##### Wildcards in Object Pattern Matching

You can use a wildcard `*` when using an object definition when performing pattern matching of object content. Wildcards can be used to
match any object properties. Consider the following pattern matching script that matches properties of an object and puts the values in
the corresponding variable references:

	{
		"name":Required(Str(Name)),
		"age":Optional(Int(Age)),
		"profession":Optional(Str(Profession)),
		"employedSince":Required(DateTime(EmployedSince))
	}
	:=
	{
		"name":"Kalle",
		"age":50,
		"profession":"Bus Driver",
		"employedSince":DateTime(2010,1,02)
	};

But if you are only interested in the name and the date when the person was employed, you can use wildcards to ignore the other elements 
and attributes:

	{
		"name":Required(Str(Name)),
		"employedSince":Required(DateTime(EmployedSince)),
		*
	}
	:=
	{
		"name":"Kalle",
		"age":50,
		"profession":"Bus Driver",
		"employedSince":DateTime(2010,1,02)
	};

### Suffix-operators

Suffix-operators are written after the operand to which they are applied. The following table lists available suffix operators:

| Operator      | Description                                 | Example                |
|:-------------:|:--------------------------------------------|:----------------------:|
| `.`           | Member operator                             | `obj.Member`           |
| `(` List `)`  | Function evaluation                         | `f(a,b,c)`             |
| `[]`          | To vector, or Array, if not already         | `a[]`, `System.Byte[]` |
| `[Index]`     | Vector index operator                       | `v[i]`                 |
| `[X,Y]`       | Matrix index operator                       | `M[x,y]`               |
| `[X,]`        | Matrix colum vector operator                | `M[x,]`                |
| `[,Y]`        | Matrix row vector operator                  | `M[,y]`                |
| `[,]`         | To matrix, if not already                   | `a[,]`                 |
| `{}`          | To set, if not already                      | `a{}`                  |
| `++`          | Post-Increment                              | `a++`                  |
| `--`          | Post-Decrement                              | `a--`                  |
| `%`           | Percent                                     | `10%`                  |
| `‰`           | Per thousand                                | `20‰`                  |
| `%0`          | Per thousand                                | `20%0`                 |
| `‱`          | Per ten thousand                            | `30‱`                |
| `‰0`          | Per ten thousand                            | `30‰0`                 |
| `%00`         | Per ten thousand                            | `30%00`                |
| `°`           | Degrees to radians                          | `sin(100°)`            |
| `'`           | Default differentiation (prim)              | `f'(x)`                |
| `′`           | Default differentiation (prim)              | `f′(x)`                |
| `"`           | Default second-order differentiation (bis)  | `f"(x)`                |
| `″`           | Default second-order differentiation (bis)  | `f″(x)`                |
| `‴`           | Default third-order differentiation         | `f‴(x)`                |
| `T`           | Transposed matrix                           | `M T`                  |
| `H`           | Conjugate Transposed matrix                 | `M H`                  |
| `†`           | Conjugate Transposed matrix                 | `M†`                   |
| `!`           | Faculty                                     | `n!`                   |
| `!!`          | Semi-Faculty                                | `n!!`                  |
| Physical unit | Defines a physical quantity.                | `10 m/s`               |

Some suffix operators can be prefixed by a `?` character, to include a *null check* of the operand. If the operand is `null`, the operator
is not evaluated, and `null` is returned. The following table lists null-checked suffix operators:

| Operator       | Description                                 | Example       |
|:--------------:|:--------------------------------------------|:-------------:|
| `?.`           | Member operator                             | `obj?.Member` |
| `?(` List `)`  | Function evaluation                         | `f?(a,b,c)`   |
| `?[]`          | To vector, if not already                   | `a?[]`        |
| `?[Index]`     | Vector index operator                       | `v?[i]`       |
| `?[X,Y]`       | Matrix index operator                       | `M?[x,y]`     |
| `?[X,]`        | Matrix colum vector operator                | `M?[x,]`      |
| `?[,Y]`        | Matrix row vector operator                  | `M?[,y]`      |
| `?[,]`         | To matrix, if not already                   | `a?[,]`       |
| `?{}`          | To set, if not already                      | `a?{}`        |

**Note**: Since script is late-bound, most operators support dynamic and static bindings, where traditional languages only support static bindings.
The following is permitted, for example:

	s:="A";
	Obj.("Property"+s):=10;

The above is the same as writing

	Obj.PropertyA:=10;	

Canonical extensions are also possible. Example:

	[Obj1,Obj2].Member

Is evaluated as (unless the vector contains a property of that name, such as the `Length` property):

	[Obj1.Member,Obj2.Member]

Canonical extensions are allowed, on both left side, and right side. Example:

	Obj.["member1","member2"]

Is evaluated as:

	[Obj1.member1,Obj2.member2]

**Note 2**: You can combine default differentiation operators to create higher order differentiation operators. 
Example `f''''(x)` represents the fourth-order differentiation operator, and is the same as `f""(x)`.

**Note 3**: For more information about physical units, see the section [Physical Quantities](#physicalQuantities) below.

### Unary prefix operators

Unary prefix operators are written before the operand to which they are applied. The following table lists available unary prefix operators:

| Operator   | Description        | Example |
|:----------:|:-------------------|:-------:|
| `++`       | Pre-Increment      | `++a`   |
| `--`       | Pre-Decrement      | `--a`   |
| `+`        | Positive (ignored) | `+a`    |
| `-`        | Negation           | `-a`    |
| `!`        | Logical Not        | `!a`    |
| `NOT`      | Logical Not        | `NOT a` |
| `~`        | Complement         | `~a`    |

### Powers

There are two power-level operators. Both have the same [order of presedence][].

| Operator   | Description        | Example  |
|:----------:|:-------------------|:--------:|
| `^`        | Power              | `a ^ b`  |
| `.^`       | Element-wise Power | `a .^ b` |

There are also a couple of special characters that are understood as power operators:

| Operator   | Description                                 | Example     |
|:----------:|:--------------------------------------------|:-----------:|
| `²`        | Square                                      | `a²`        |
| `³`        | Cube                                        | `a³`        |

### Factors

There are several factor-level operators, apart from the assignment versions. Both have the same [order of presedence][].

| Operator    | Description                      | Example         |
|:-----------:|:---------------------------------|:---------------:|
| `*`         | Multiplication                   | `a * b`         |
| `/`         | Division                         | `a / b`         |
| `\`         | Left-Division, or set difference | `a \ b`, `A\B`  |
| `MOD`       | Residue (modulus)                | `a MOD b`       |
| `.MOD`      | Element-wise Residue             | `a .MOD b`      |
| `.*`        | Element-wise Multiplication      | `a .* b`        |
| `./`        | Element-wise Division            | `a ./ b`        |
| `.\`        | Element-wise Left-Division       | `a .\ b`        |
| `DOT`       | Dot product                      | `a DOT b`       |
| `CROSS`     | Cross product                    | `a CROSS b`     |
| `CARTESIAN` | Cartesian product                | `a CARTESIAN b` |

**Note**: In some languages `%` is a residue operator. In this language, the `%` operator is a [percent operator](#unarySuffixOperators).

### Binomial Coefficients

Binomial coefficients can be calculated using the `OVER` operator, as follows:

	n OVER k

### Terms

There are four term-level operators, apart from the assignment versions. Both have the same [order of presedence][].

| Operator   | Description              | Example  |
|:----------:|:-------------------------|:--------:|
| `+`        | Addition                 | `a + b`  |
| `-`        | Subtraction              | `a - b`  |
| `.+`       | Element-wise Addition    | `a .+ b` |
| `.-`       | Element-wise Subtraction | `a .- b` |

### Intervals

Intervals can be created in the following way:

	From .. To

By default, the *step size* is 1. You can specify the step size in the following way:

	From .. To | StepSize

### Intersections

The intersection of two sets is accomplished as follows:

	Set1 INTERSECT Set2.
	Set1 INTERSECTION Set2.

The Intersection character `∩` can also be used:

	Set1 ∩ Set2

### Unions

The union of two sets is accomplished as follows:

	Set1 UNION Set2.

The CUP character `∪` can also be used:

	Set1 ∪ Set2

### Shift operators

There are two shift operators, apart from the assignment versions. Both have the same [order of presedence][].

| Operator   | Description | Example  |
|:----------:|:------------|:--------:|
| `<<`       | Shift left  | `a << b` |
| `>>`       | Shift right | `a >> b` |

### Comparison operators

There are various different comparison operators. All have the same [order of presedence][].

| Operator   | Description                       | In Pattern Matching  | Example            |
|:----------:|:----------------------------------|:---------------------|:------------------:|
| `<=`       | Lesser than or equal to           | Asserts valid range. | `a <= b`           |
| `<`        | Lesser than                       | Asserts valid range. | `a < b`            |
| `>=`       | Greater than or equal to          | Asserts valid range. | `a >= b`           |
| `>`        | Greater than                      | Asserts valid range. | `a > b`            |
| `=`        | Equal to                          | Asserts valid range. | `a = b`            |
| `==`       | Equal to                          | Asserts valid range. | `a == b`           |
| `===`      | Identical to                      |                      | `a === b`          |
| `<>`       | Not Equal to                      | Asserts valid range. | `a <> b`           |
| `!=`       | Not Equal to                      | Asserts valid range. | `a != b`           |
| `LIKE`     | Matches regular expression        | Asserts valid range. | `a LIKE regex`     |
| `NOT LIKE` | Does not match regular expression | Asserts valid range. | `a NOT LIKE regex` |
| `NOTLIKE`  | Does not match regular expression | Asserts valid range. | `a NOTLIKE regex`  |
| `UNLIKE`   | Does not match regular expression | Asserts valid range. | `a UNLIKE regex`   |
| `.=`       | Equal to (element-wise)           |                      | `a .= b`           |
| `.==`      | Equal to (element-wise)           |                      | `a .== b`          |
| `.===`     | Identical to (element-wise)       |                      | `a .=== b`         |
| `.<>`      | Not Equal to (element-wise)       |                      | `a .<> b`          |
| `.!=`      | Not Equal to (element-wise)       |                      | `a .!= b`          |

**Note**: Element-wise variant of operators only exist for equality or non-equality operators, since these are also defined when comparing encapsulating
objects such as sets, vectors, arrays, matrices, etc.

**Note 2**: If regular expressions contain named groups, variables with the corresponding names will be set to the contents of the corresponding
groups if the regular expression matches the string. If the contents is numerical, the corresponding variable will also be numerical. If you
want to reference the string literal, use the variable starting with the same name having a suffix of `_STR`. The position of a group variable
match is provided by the variable starting with the same name having a suffix of `_POS`. Likewise, the variable starting with the same name having 
a suffix of `_LEN` provides the length of the match.

#### Range operators

You can combine two of the `<` and `<=`, or `>` and `>=` to create range operators. Examples:

	10 < x <= 20
	100 >= y >= 0
	{"name":Str(Required(Name)),"age":Required(0 <= Int(Age) <= 100)),"profession":Optional(Str(Profession))}:=Obj

### Membership operators

There are various different membership operators. All have the same [order of presedence][].

| Operator   | Description                       | Example            |
|:----------:|:----------------------------------|:------------------:|
| `AS`       | The `AS` operator makes sure the left operand is of the same type as the right operand. The result is **null** if they are not, or the same value as the left operand if they are.           | `Value as Type`           |
| `IS`       | The `IS` operator checks if the left operand is of the same type as the right operand. | `Value is Type` |
| `IS NOT`   | The `IS NOT` operator checks if the left operand is not of the same type as the right operand. | `Value is not Type` |
| `INHERITS` | The `INHERITS` operator checks if the left operand inherits the type defined by the right operand. | `Value inherits Type` |
| `IN`       | The `IN` operator checks if the left operand is a member of the right operand. | `Value in Set` |
| `MATCHES`  | The `MATCHES` operator checks if the left operand matches the construct of the right operand using [pattern matching](#assignmentPatternMatching), and return `true` of the left operand matches the right, and `false` otherwise. If there's a match, any implicit variables are set accordingly. | `v matches [a,b,c]` |
| `NOT IN`   | The `NOT IN` or `NOTIN` operator checks if the left operand is not a member of the right operand. | `Value not in Set` |
| `NOTIN`    | The `NOT IN` or `NOTIN` operator checks if the left operand is not a member of the right operand. | `Value notin Set` |

### AND operators

There are various different AND operators. All have the same [order of presedence][].

| Operator   | Description                       | Example            |
|:----------:|:----------------------------------|:------------------:|
| `&`        | To specify an explicit logical AND operator, use the `&` operator. | `a & b` |
| `&&`       | To specify an explicit binary AND operator, use the `&&` operator. | `a && b` |
| `AND`      | The `AND` operator (case insensitive) works differently depending on the values being operated on. If they are boolean values, the operator works as a logical operator. If they are integers, the operator works as a binary operator. | `a and b` |
| `NAND`     | The `NAND` operator (case insensitive), or the not-and operator, works differently depending on the values being operated on. If they are boolean values, the operator works as a logical operator. If they are integers, the operator works as a binary operator. | `a nand b` |

### OR operators

There are various different OR operators. All have the same [order of presedence][].

| Operator                  | Description                       | Example            |
|:-------------------------:|:----------------------------------|:------------------:|
| <code>&#124;</code>       | To specify an explicit logical OR operator, use the <code>&#124;</code> operator. | <code>a &#124; b</code> |
| <code>&#124;&#124;</code> | To specify an explicit binary OR operator, use the <code>&#124;&#124;</code> operator. | <code>a &#124;&#124; b</code> |
| `OR`                      | The `OR` operator (case insensitive) works differently depending on the values being operated on. If they are boolean values, the operator works as a logical operator. If they are integers, the operator works as a binary operator. | `a or b` |
| `NOR`                     | The `NOR` operator (case insensitive), or the not-or operator, works differently depending on the values being operated on. If they are boolean values, the operator works as a logical operator. If they are integers, the operator works as a binary operator. | `a nor b` |
| `XOR`                     | The `XOR` operator (case insensitive) works differently depending on the values being operated on. If they are boolean values, the operator works as a logical operator. If they are integers, the operator works as a binary operator. | `a xor b` |
| `XNOR`                    | The `XNOR` operator (case insensitive), or the not-xor operator, works differently depending on the values being operated on. If they are boolean values, the operator works as a logical operator. If they are integers, the operator works as a binary operator. | `a xnor b` |

**Note**: The XNOR operator has the same truth table as the *equivalence* operator `<=>`, but the [order of presedence][] is different.

### Equivalence or implication

Equivalence or implication operators have the same [order of presedence][].

#### Equivalence

The equivalence operator is a boolean operator that checks if the both sides are equivalent (or equal, as boolean values). Example:

	a <=> b

Truth table:

| `<=>` | ⊤ | ⊥ |
|:-----:|:-:|:--:|
| ⊤     | ⊤ | ⊥ |
| ⊥     | ⊥ | ⊤ |

#### Implication

The implication operator is a boolean operator that checks if the left side implies the right side (as boolean values). Example:

	a => b

Truth table:

| `=>`   | ⊤ | ⊥ |
|:------:|:-:|:--:|
| ⊤      | ⊤ | ⊥ |
| ⊥      | ⊤ | ⊤ |

### Lambda definition

A lambda definition creates an implicit unnamed function. A lambda definition is created by using the `->` operator. If multiple parameters
are used, they must be enclosed between parenthesis. Examples:

	x->x^2;
	(x,y)->sin(x)*exp(-1/y^2);

**Note**: Implicit lambda definitions are created if referring to a function by using only its name. If writing only `abs`, a lambda definition
`x->abs(x)` will be returned. This makes it possible to create functions taking lambda expressions as parameters, implementing algorithms, and call them
using simple references to the existing function library.

#### Lambda definitions and canonical extensions

When defining lambda functions you can provide information on how arguments are to be treated, and thus also implicitly automatically define canonical 
extensions for the expression. Each argument `x` can be defined to belong to one of five categories:

| Argument | Category                   | Example              |
|:--------:|:---------------------------|:--------------------:|
| `x`      | Normal argument. When the expression is called, arguments are passed as they are given. | `x->...`  |
| `[x]`    | Scalar argument. When the expression is called with non-scalar arguments such as vectors and matrices, the function is canonically extended by calling the function repeatedly for each scalar element, and returning a structure similar to the structure of the argument. | `[x]->...`  |
| `x[]`    | Vector argument. When the expression is called with scalar arguments, they are converted to one-dimensional vectors. If matrix arguments are used, the function is canonically extended by calling the function repeatedly for each row vector of the matrix, and returning a structure similar to the structure of the argument. If a set is passed, the extension loops through the elements of the set canonically. | `v[]->...`  |
| `x{}`    | Set argument. When the expression is called with scalar arguments, they are converted to one-dimensional sets. If matrix arguments are used, the function is canonically extended by calling the function repeatedly for each row vector of the matrix, and returning a structure similar to the structure of the argument. If a vector is passed, it is converted to a set.  | `v{}->...`  |
| `x[,]`   | Matrix argument. When the expression is called with scalar or vector arguments, they are converted to 1x1 matrices or matrices consisting of only one row. If a set is passed, the extension loops through the elements of the set canonically. | `M[,]->...`  |

### Assignment

A variable assignment is defined using the `:=` operator. Example:

	Variable := Value

#### Assignment, Pattern Matching

If the left side is not a variable reference, a pattern matching algorithm is employed that tries to assign all implicitly available variable references
by comparing both sides.

Examples:

	[x,y]:=f(a,b,c)
	v[]:=f(a,b,c)
	[[a11,a12],[a21,a22]]:=Identity(2)
	{"name":Str(Required(Name)),"age":Required(0 <= Int(Age) <= 100)),"profession":Optional(Str(Profession))}:=Obj

In the first example, the function `f`, which takes three parameters, is supposed to return a vector of two elements. If it does, 
the variables `x` and `y` are assigned the elements of this return vector. In the second example, the `v` is supposed to be a assigned
a vector. If the result of the function call is not a vector, it is converted to a vector before being assigned to `v`. The third
example matches the 2x2 matrix result evaluated on the right, with the matrix notation on the left. The fourth example matches an
object created ex-nihilo (such as a JSON object), with an object definition, including checks of data types (and possible conversions),
tests of valid values, as well as checks for required and optional fields.

The `MATCHES` operator can also be used. It returns `true` if there's a pattern match (and implicit variables are assigned 
accordingly), or `false`if there's not a pattern match. The above examples become:

	f(a,b,c) matches [x,y]
	f(a,b,c) matches v[]
	Identity(2) matches [[a11,a12],[a21,a22]]
	Obj matches {"name":Str(Required(Name)),"age":Required(0 <= Int(Age) <= 100)),"profession":Optional(Str(Profession))}

#### Special Assignment Operators

There's also a set of aritmethic operators that act directly on a variable value, an object member or index. These are also categorized as 
assignment operators, and have the same [order of presedence][]. These operators are:

| Operator                   | Meaning                   | Example                                   |
|:--------------------------:|:--------------------------|:-----------------------------------------:|
| `+=`                       | Add to variable           | `Variable += Value`                       |
| `-=`                       | Subtract from variable    | `Variable -= Value`                       |
| `*=`                       | Multiply to variable      | `Variable *= Value`                       |
| `/=`                       | Divide from variable      | `Variable /= Value`                       |
| `^=`                       | Power variable            | `Variable ^= Value`                       |
| `&=`                       | Binary AND with variable  | `Variable &= Value`                       |
| `&&=`                      | Logical AND with variable | `Variable &&= Value`                      |
| <code>&#124;=</code>       | Binary OR with variable   | <code>Variable &#124;= Value</code>       |
| <code>&#124;&#124;=</code> | Logical OR with variable  | <code>Variable &#124;&#124;= Value</code> |
| `<<=`                      | Shift variable left       | `Variable <<= Value`                      |
| `>>=`                      | Shift variable right      | `Variable >>= Value`                      |

**Note**: `^=` is not a logical XOR with self operator but a power of self operator.

You can also combine operators to perform partial assignments, as follows:

| Operator | Meaning                   | Example              |
|:--------:|:--------------------------|:--------------------:|
| ... `.` ... `:=` | Membership assignment | `Obj.Member := Value`  |
| ... `[` ... `]:=` | Vector index assignment | `Vector[Index] := Value`  |
| ... `[` ... `,` ... `]:=` | Matrix index assignment | `M[x,y] := Value`  |
| ... `[` ... `,]:=` | Matrix column assignment | `M[x] := Value`  |
| ... `[,` ... `]:=` | Matrix row assignment | `M[,y] := Value`  |
| ... `(` ... `):=` | Function definition | `f(x,y,z) := x*y*z`  |

#### Function definitions and canonical extensions

When defining functions you can provide information on how arguments are to be treated, and thus also implicitly automatically define canonical extensions
for the function. Each argument `x` can be defined to belong to one of five categories:

| Argument | Category                   | Example              |
|:--------:|:---------------------------|:--------------------:|
| `x`      | Normal argument. When the expression is called, arguments are passed as they are given. | `f(x):=...`  |
| `[x]`    | Scalar argument. When the expression is called with non-scalar arguments such as vectors and matrices, the function is canonically extended by calling the function repeatedly for each scalar element, and returning a structure similar to the structure of the argument. | `f([x]):=...`  |
| `x[]`    | Vector argument. When the expression is called with scalar arguments, they are converted to one-dimensional vectors. If matrix arguments are used, the function is canonically extended by calling the function repeatedly for each row vector of the matrix, and returning a structure similar to the structure of the argument. If a set is passed, the extension loops through the elements of the set canonically. | `f(v[]):=...`  |
| `x{}`    | Set argument. When the expression is called with scalar arguments, they are converted to one-dimensional sets. If matrix arguments are used, the function is canonically extended by calling the function repeatedly for each row vector of the matrix, and returning a structure similar to the structure of the argument. If a vector is passed, it is converted to a set.  | `f(v{}):=...`  |
| `x[,]`   | Matrix argument. When the expression is called with scalar or vector arguments, they are converted to 1x1 matrices or matrices consisting of only one row. If a set is passed, the extension loops through the elements of the set canonically. | `f(M[,]):=...`  |

### Conditional IF

Conditional `IF`-statements can be written in various ways. Either using the `IF` and `THEN` keywords, followed by the optional `ELSE` 
keyword, or by using the `?` operator, followed by the optional `:` operator. There is also a quick null-check statement using the
`??` operator. There is a `???` operator also, described in the following section.

Examples:

	IF Condition THEN IfTrueStatement
	IF Condition THEN IfTrueStatement ELSE IfFalseStatement
	Condition ? IfTrueStatement
	Condition ? IfTrueStatement : IfFalseStatement
	Statement ?? IfNullStatement

**Note**: `IF`, `THEN` and `ELSE` are case insensitive. They are written here using upper case for clarity.

**Note 2**: If no `ELSE` or `:` is present, the statement is evaluated to **null**.

### Conditional Statements (DO/WHILE, WHILE/DO, FOR, FOREACH, TRY CATCH FINALLY)

There are multiple ways to execute conditional loops. These statements have the same [order of presedence][]:

| Operator | Meaning                   | Example              |
|:--------:|:--------------------------|:--------------------:|
| `DO` ... `WHILE` ... | Performs an action until a condition becomes **true**. | `DO Statement WHILE Condition` |
| `WHILE` ... [`DO`] ... | While a condition is **true**, performs an action. | `WHILE Condition DO Statement` |
| `FOREACH` ... `IN` ... [`DO`] ... | Iterates a variable through an enumerable set of values and performs an action on each iterated value. | `FOREACH Variable in Collection DO Statement` |
| `FOR EACH` ... `IN` ... [`DO`] ... | Iterates a variable through an enumerable set of values and performs an action on each iterated value. | `FOR EACH Variable in Collection DO Statement` |
| `FOR` ... `:=` ... `TO` ... [`STEP` ...] [`DO`] ... | Iterates a variable through a sequence of numerical values and performs an action on each iterated value. | `FOR Variable:=From TO Stop STEP StepSize DO Statement` |
| `TRY` ... `CATCH` ... `FINALLY` ... | Executes a statement. If an exception occurs, it is caught and an exception statement is executed. Afterwards, a finalization statement is executed. The exception object will be available in the CATCH statement, under the name of `Exception`. | `TRY Statement CATCH Exception FINALLY Done` |
| `TRY` ... `CATCH` ... | Executes a statement. If an exception occurs, it is caught and an exception statement is executed. The exception object will be available in the CATCH statement, under the name of `Exception`. | `TRY Statement CATCH Exception` |
| ... `???` ... | Short form for `TRY` ... `CATCH` ... | `... ??? Exception` |
| `TRY` ... `FINALLY` ... | Executes a statement. Afterwards, a finalization statement is executed regardless if an exception has been thrown or not. Any exceptions are automatically propagated. | `TRY Statement FINALLY Done` |
| `]]`...`[[` | Implicit print statement. This operation prints the contents between the `]]` and `[[` to the current console output. Any expressions embedded between `((` and `))` will be evaluated and the result displayed. | `a:=10;]]Value of a: ((a)).[[;` |

**Note**: The `DO` keyword can be replaced by a `:`. 

**Note 2**: The use of the `STEP` keyword together with the step size is optional. If omitted, a default step size of `1` or `-1` will be used, depending
if the loop is ascending or descending.

**Note 3**: Exceptions caught using the `CATCH` statement are accessible within the `CATCH` statement by referencing the variable `Exception`.

**Note 4**: You can use the `Break([x])` function to break out of a loop (`DO` ... `WHILE`, 
`WHILE` ... `DO`, `FOREACH`, `FOR EACH`, and `FOR`), and the `Continue([x])` 
function to skip to the next iteration. If a value is provided to the `Break` function, the value 
will be considered the last value of the loop. If a value is provided to the `Continue` function,
the value will be considered the value of the current iteration of the loop. The last value
produced will be the value returned from the loop statement.

**Note 5**: There is a performance penalty of using `Break` and `Continue`, compared to using
conditional statements within the loop. Internally, `Continue` and `Break` use exceptions to exit
from evaluation in the parsed syntax tree. The performance penalty can be as large as 100 times,
and should only be used if a conditional statement is not possible, or too complex to create.

### Lists

Lists of statements are created by writing a list of statements or arguments, each separated by a comma `,` character. Example:

	Statement1, Statement2, Statement3, ...
	
Whitespace is ignored. This includes new-line characters. So Statements can be written on separate rows. Example:

	Statement1,
	Statement2,
	Statement3,
	... 

Lists are used in different constructs, such as *function* definitions and evaluations, *lambda* definitions, or when working with 
sets, vectors or matrices, for example.

It is permissible to ignore arguments in a list. Such arguments will implicitly receive the value **null**. Example:

	Arg1, Arg2,, Arg4

Here, there third argument will have value **null**.

### Sequences

Sequences of statements are created by writing a list of statements, each separated by a semicolon `;` character. Example:

	Statement1; Statement2; Statement3; ...
	
Whitespace is ignored. This includes new-line characters. So Statements can be written on separate rows. Example:

	Statement1;
	Statement2;
	Statement3;
	... 

=========================================================================================================================================================

Functions
---------------------

Functions are extensible and can be defined in any module in the solution. A complete list of functions available in a solution therefore
depends on all libraries included in the project. Functions listed here only include functions defined in this library.

**Note**: Function names are *case insensitive*.

### Analytic Functions

The following subsections list available analytic or partially analytic functions, partitioned into groups.

#### Exponential and power functions

The following table lists available exponential and power functions:

| Function   | Description | Example |
|------------|-------------|---------|
| `Exp(z)`   | `e` raised to the power of `z`. | `Exp(10)` |
| `Ln(z)`    | Natural logarithm of `z`.       | `Ln(e)`   |
| `Lg(z)`    | Base-10 logarithm of `z`.       | `Lg(10)`  |
| `Log10(z)` | Alias for `lg`.                 | `Lg(10)`  |
| `Log2(z)`  | Base-2 logarithm of `z`.        | `Log2(2)` |
| `Sqrt(z)`  | Square root of `z`.             | `Sqrt(2)` |

#### Trigonometric functions

The following table lists available trigonometric functions:

| Function       | Description | Example |
|----------------|-------------|---------|
| `Cos(z)`       | Cosine, `z` in radians. | `Cos(100°)` |
| `CosSin(z)`    | Cosine and Sine, `z` in radians. | `CosSin(100°)` |
| `Cot(z)`       | Cotangent, `z` in radians. | `Cot(100°)` |
| `Csc(z)`       | Cosecant, `z` in radians. | `Csc(100°)` |
| `Sec(z)`       | Secant, `z` in radians. | `Sec(100°)` |
| `Sin(z)`       | Sine, `z` in radians. | `Sin(100°)` |
| `SinCos(z)`    | Sine and Cosine, `z` in radians. | `SinCos(100°)` |
| `Tan(z)`       | Tangent, `z` in radians. | `Tan(100°)` |
| `ACos(z)`      | Alias for `ArcCos(z)`. | `ACos(Cos(100°))` |
| `ACot(z)`      | Alias for `ArcCot(z)`. | `ACot(Cot(100°))` |
| `ACsc(z)`      | Alias for `ArcCsc(z)`. | `ACsc(Csc(100°))` |
| `ASec(z)`      | Alias for `ArcSec(z)`. | `ASec(Sec(100°))` |
| `ASin(z)`      | Alias for `ArcSin(z)`. | `ASin(Sin(100°))` |
| `ATan(z)`      | Alias for `ArcTan(z)`. | `ATan(Tan(100°))` |
| `ATan(x,y)`    | Alias for `ArcTan(x,y)`. | `ATan(3,4)` |
| `ArcCos(z))`   | Inverse Cosine. | `ArcCos(Cos(100°))` |
| `ArcCot(z))`   | Inverse Cotangent. | `ArcCot(Cot(100°))` |
| `ArcCsc(z))`   | Inverse Cosecant. | `ArcCsc(Csc(100°))` |
| `ArcSec(z))`   | Inverse Secant. | `ArcSec(Sec(100°))` |
| `ArcSin(z)`    | Inverse Sine. | `ArcSin(Sin(100°))` |
| `ArcTan(z))`   | Inverse Tangent. | `ArcTan(Tan(100°))` |
| `ArcTan(x,y))` | Returns the angle whose tangent is the quotient of two specified numbers. | `ArcTan(3,4)` |

#### Hyperbolic functions

A corresponding set of hyperbolic functions also exists:

| Function | Description | Example |
|----------|-------------|---------|
| `CosH(z)` | Hyperbolic Cosine, `z` in radians. | `CosH(100°)` |
| `CotH(z)` | Hyperbolic Cotangent, `z` in radians. | `CotH(100°)` |
| `CscH(z)` | Hyperbolic Cosecant, `z` in radians. | `CscH(100°)` |
| `SecH(z)` | Hyperbolic Secant, `z` in radians. | `SecH(100°)` |
| `SinH(z)` | Hyperbolic Sine, `z` in radians. | `SinH(100°)` |
| `TanH(z)` | Hyperbolic Tangent, `z` in radians. | `TanH(100°)` |
| `ACosH(z)` | Alias for `ArcCosH(z)`. | `ACosH(CosH(100°))` |
| `ACotH(z)` | Alias for `ArcCotH(z)`. | `ACotH(CotH(100°))` |
| `ACscH(z)` | Alias for `ArcCscH(z)`. | `ACscH(CscH(100°))` |
| `ASecH(z)` | Alias for `ArcSecH(z)`. | `ASecH(SecH(100°))` |
| `ASinH(z)` | Alias for `ArcSinH(z)`. | `ASinH(SinH(100°))` |
| `ATanH(z)` | Alias for `ArcTanH(z)`. | `ATanH(TanH(100°))` |
| `ArcCosH(z))` | Inverse Hyperbolic Cosine. | `ArcCosH(CosH(100°))` |
| `ArcCotH(z))` | Inverse Hyperbolic Cotangent. | `ArcCotH(CotH(100°))` |
| `ArcCscH(z))` | Inverse Hyperbolic Cosecant. | `ArcCscH(CscH(100°))` |
| `ArcSecH(z))` | Inverse Hyperbolic Secant. | `ArcSecH(SecH(100°))` |
| `ArcSinH(z)` | Inverse Hyperbolic Sine. | `ArcSinH(SinH(100°))` |
| `ArcTanH(z))` | Inverse Hyperbolic Tangent. | `ArcTanH(TanH(100°))` |

### Scalar Functions

The following table lists available scalar functions:

| Function     | Description                               | In Pattern Matching                                     | Example           |
|--------------|-------------------------------------------|---------------------------------------------------------|-------------------|
| `Abs(z)`     | Absolute value (or magnitude of) `z`      | Asserts `z` is non-negative.                            | `Abs(-1)`         |
| `Bool(x)`    | Alias for `Boolean`.                      | Asserts `x` is a Boolean.                               | `Bool('true')`    |
| `Boolean(x)` | Converts `x` to a boolean value.          | Asserts `x` is a Boolean.                               | `Boolean('true')` |
| `Ceiling(z)` | Round `z` up to closest integer.          | Asserts `z` is an Integer.                              | `Ceiling(pi)`     |
| `Ceil(z)`    | Alias for `Ceiling(z)`.                   | Asserts `z` is an Integer.                              | `Ceil(-1)`        |
| `Dbl(x)`     | Alias for `Double(x)`                     | Asserts `x` is a double-precision floating-point value. | `Dbl("3.14")`     |
| `Double(x)`  | Converts `x` to a double value.           | Asserts `x` is a double-precision floating-point value. | `Double(pi)`      |
| `Floor(z)`   | Round `z` down to closest integer.        | Asserts `z` is an Integer.                              | `Floor(pi)`       |
| `Frac(z)`    | Returns the decimals (fractions) of `z`.  |                                                         | `Frac(pi)`        |
| `Guid(x)`    | Converts `x` to a GUID value.             | Asserts `x` is a GUID.                                  | `Guid(s)`         |
| `Int(x)`     | Alias for `Integer(x)`                    | Asserts `x` is an Integer.                              | `Int(pi)`         |
| `Integer(x)` | Round `x` down to closest integer.        | Asserts `x` is an Integer.                              | `Integer(pi)`     |
| `Max(x,y)`   | Largest of `x` and `y`.                   |                                                         | `Max(10,a)`       |
| `Min(x,y)`   | Smallest of `x` and `y`.                  |                                                         | `Min(10,a)`       |
| `NewGuid()`  | Creates a new GUID.                       |                                                         | `NewGuid()`       |
| `Num(x)`     | Alias for `Number(x)`.                    | Asserts `x` is a number.                                | `Num('100')`      |
| `Number(x)`  | Converts `x` to a number.                 | Asserts `x` is a number.                                | `Number('100')`   |
| `Round(z)`   | Round `z` up or down to closest integer.  | Asserts `z` is an Integer.                              | `Round(pi)`       |
| `Sign(z)`    | Sign of `z` (-1/0/1 + -i/0/+i).           | Asserts `z` is -1, 0 or 1.                              | `Sign(pi)`        |
| `Str(x)`     | Alias for `String(x)`.                    | Asserts `x` is a string.                                | `Str(100)`        |
| `String(x)`  | Converts `x` to a string.                 | Asserts `x` is a string.                                | `String(100)`     |
| `Uri(x)`     | Converts `x` to an URI object value.      | Asserts `x` is a URI.                                   | `Uri(s)`          |

### Complex Functions

The following table lists available scalar functions:

| Function       | Description                                            | Example          |
|----------------|--------------------------------------------------------|------------------|
| `Arg(z)`       | Argument (or phase) of `z`.                            | `Arg(2+i)`       |
| `Conj(z)`      | Alias for `Conjugate(z)`.                              | `Conj(2+i)`      |
| `Conjugate(z)` | Conjugate of `z`.                                      | `Conjugate(2+i)` |
| `Im(z)`        | Imaginary part of `z`.                                 | `Im(2+i)`        |
| `Polar(n,φ)`   | Complex number given in polar coordinates `n` and `φ`. | `Polar(1,pi/2)`  |
| `Re(z)`        | Real part of `z`.                                      | `Re(2+i)`        |

### String Functions

The following table lists available string-related functions:

| Function                          | Description | Example |
|-----------------------------------|-------------|---------|
| `After(s,Delimiter)`              | Returns the part of the string that occurs after the last occurrence of the Delimiter string. | `Port:=Num(After(EP,":"))` |
| `Before(s,Delimiter)`             | Returns the part of the string that occurs before the first occurrence of the Delimiter string. | `IP:=Before(EP,":")` |
| `Concat(v[,Delimiter})`           | Concatenates the elements of a vector, optionally delimiting the elements with `Delimiter`. If `v` is not a vector, it is returned, as-is. | `Concat(Elements,",")` |
| `Contains(s,Substring)`           | Returns `true` if `s` contains `Substring` somewhere, `false` otherwise. | `Contains(s,"Hello")` |
| `Empty(s)`                        | Alias for `IsEmpty(s)`. | `Empty(s)` |
| `EndsWith(s,Substring)`           | Returns `true` if `s` ends with `Substring`, `false` otherwise. | `EndsWith(s,"Hello")` |
| `Eval(s)`                         | Alias for `Evaluate(s)`. | `Evaluate("a+b")` |
| `Evaluate(s)`                     | Parses the string and evaluates it. | `Evaluate("a+b")` |
| `IndexOf(s,Substring[,From])`     | Returns the zero-based index of `Substring` in `s`. If `Substring` is not found in `s`, `-1` is returned. If `From` is provided, search starts from this position. | `IndexOf("Hello","el")` |
| `IsEmpty(s)`                      | Returns a boolean value showing if the string `s` is empty or not. | `IsEmpty(s)` |
| `LastIndexOf(s,Substring[,From])` | Returns the last zero-based index of `Substring` in `s`. If `Substring` is not found in `s`, `-1` is returned. If `From` is provided, search starts from this position. | `LastIndexOf("Hello","el")` |
| `Left(s,N)`                       | Returns a string with the left-most `N` characters. If the string `s` is shorter, the entire string is returned. | `Left(s,3)` |
| `Len(s)`                          | Alias for `Length(s)`. | `Len(s)` |
| `Length(s)`                       | Returns the length of the string. | `Length(s)` |
| `LowerCase(s)`                    | Returns the lower-case version of `s`. | `LowerCase("Hello")` |
| `Mid(s,Pos,Len)`                  | Returns a substring of `s`, starting a character `Pos` and continuing `Len` characters. The `Pos` index is zero-based. If the requested substring goes beyond the scope of `s`, the substring gets truncated accordingly. | `Mid(s,5,2)` |
| `PadLeft(s,N)`                    | Returns the string `s` padded to the left with space characters, until it contains `N` characters. | `PadLeft("Hello",10)` |
| `PadRight(s,N)`                   | Returns the string `s` padded to the right with space characters, until it contains `N` characters. | `PadRight("Hello",10)` |
| `Parse(s)`                        | Parses the string as an expression, and returns the parsed expression. | `Parse("a+b")` |
| `Replace(s,From,To[,Options])`    | Replaces all occurrences of the `From` string in `s` with the `To` string. If `Options` are provided (can be empty string), `From` is treated as a regular expression. Supported options include combinations of letters `i`, `m` and `x`. | `Replace(s,"Hello","Bye")` |
| `Right(s,N)`                      | Returns a string with the right-most `N` characters. If the string `s` is shorter, the entire string is returned. | `Right(s,3)` |
| `Split(s,Substring)`              | Returns an array of substrings of `s`, delimited by `Substring`. | `Split("Hello World","l")` |
| `StartsWith(s,Substring)`         | Returns `true` if `s` starts with `Substring`, `false` otherwise. | `StartsWith(s,"Hello")` |
| `ToExpression(x)`                 | Returns an expression string (if possible), that if evaluated, returns a value equal to `x`. | `ToExpression(Identity(3))` |
| `Trim(s)`                         | Returns a trimmed version of `s`. | `Trim(" Hello ")` |
| `TrimEnd(s)`                      | Returns a version of `s` with its end trimmed. | `TrimEnd(" Hello ")` |
| `TrimStart(s)`                    | Returns a version of `s` with its start trimmed. | `TrimStart(" Hello ")` |
| `UpperCase(s)`                    | Returns the upper-case version of `s`. | `UpperCase("Hello")` |

### Date and Time Functions

| Function                                                       | Description                                                                                                                            | In Pattern Matching                                                                             | Example                                                     |
|----------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| `DateTime(s)`                                                  | Parses a Date and Time value in unspecified time coordinates from a string.                                                            | Asserts `s` is a date & time value.                                                             | `DateTime("2016-03-05T14:34:12.302")`                       |
| `DateTime(Ticks)`                                              | Creates a Date and Time value in unspecified time coordinates from the number of ticks it represents.                                  | Extracts the number of ticks from a date & time value.                                          | `DateTime(Now.Ticks)`                                       |
| `DateTime(Year,Month,Day)`                                     | Creates a Date value in unspecified time coordinates.                                                                                  | Extracts year, month and day from a date & time value.                                          | `DateTime(2016,03,05)`                                      |
| `DateTime(Year,Month,Day,Hour,Minute,Second)`                  | Creates a Date and Time value in unspecified time coordinates.                                                                         | Extracts year, month, day, hour, minute and second from a date & time value.                    | `DateTime(2016,03,05,19,17,23)`                             |
| `DateTime(Year,Month,Day,Hour,Minute,Second,MSecond)`          | Creates a Date and Time value in unspecified time coordinates.                                                                         | Extracts year, month, day, hour, minute, second and millisecond from a date & time value.       | `DateTime(2016,03,05,19,17,23,123)`                         |
| `DateTimeLocal(s)`                                             | Parses a Date and Time value in local time coordinates from a string.                                                                  | Asserts `s` is a local date & time value.                                                       | `DateTimeLocal("2016-03-05T14:34:12.302")`                  |
| `DateTimeLocal(Ticks)`                                         | Creates a Date and Time value in local time coordinates from the number of ticks it represents.                                        | Extracts the number of ticks from a local date & time value.                                    | `DateTimeLocal(Now.Ticks)`                                  |
| `DateTimeLocal(Year,Month,Day)`                                | Creates a Date value in local time coordinates.                                                                                        | Extracts year, month and day from a local date & time value.                                    | `DateTimeLocal(2016,03,05)`                                 |
| `DateTimeLocal(Year,Month,Day,Hour,Minute,Second)`             | Creates a Date and Time value in local time coordinates.                                                                               | Extracts year, month, day, hour, minute and second from a local date & time value.              | `DateTimeLocal(2016,03,05,19,17,23)`                        |
| `DateTimeLocal(Year,Month,Day,Hour,Minute,Second,MSecond)`     | Creates a Date and Time value in local time coordinates.                                                                               | Extracts year, month, day, hour, minute, second and millisecond from a local date & time value. | `DateTimeLocal(2016,03,05,19,17,23,123)`                    |
| `DateTimeOffset(s)`                                            | Parses a Date and Time value with a Time Zone from a string.                                                                           | Asserts `s` is a UTC date & time value.                                                         | `DateTimeOffset("2016-03-05T14:34:12.302+01:00")`           |
| `DateTimeOffset(Ticks,tz)`                                     | Creates a Date and Time value with a Time Zone from the number of ticks it represents.                                                 | Extracts the number of ticks from a UTC date & time value.                                      | `DateTimeOffset(Now.Ticks,TimeSpan("01:00"))`               |
| `DateTimeOffset(Year,Month,Day,tz)`                            | Creates a Date value with a Time Zone.                                                                                                 | Extracts year, month and day from a UTC date & time value.                                      | `DateTimeOffset(2016,03,05,TimeSpan("01:00"))`              |
| `DateTimeOffset(Year,Month,Day,Hour,Minute,Second,tz)`         | Creates a Date and Time value with a Time Zone.                                                                                        | Extracts year, month, day, hour, minute and second from a UTC date & time value.                | `DateTimeOffset(2016,03,05,19,17,23,TimeSpan("01:00"))`     |
| `DateTimeOffset(Year,Month,Day,Hour,Minute,Second,MSecond,tz)` | Creates a Date and Time value with a Time Zone.                                                                                        | Extracts year, month, day, hour, minute, second and millisecond from a UTC date & time value.   | `DateTimeOffset(2016,03,05,19,17,23,123,TimeSpan("01:00"))` |
| `DateTimeUtc(s)`                                               | Parses a Date and Time value in Universal Time Coordinates from a string.                                                              | Asserts `s` is a UTC date & time value.                                                         | `DateTimeUtc("2016-03-05T14:34:12.302")`                    |
| `DateTimeUtc(Ticks)`                                           | Creates a Date and Time value in Universal Time Coordinates from the number of ticks it represents.                                    | Extracts the number of ticks from a UTC date & time value.                                      | `DateTimeUtc(Now.Ticks)`                                    |
| `DateTimeUtc(Year,Month,Day)`                                  | Creates a Date value in Universal Time Coordinates.                                                                                    | Extracts year, month and day from a UTC date & time value.                                      | `DateTimeUtc(2016,03,05)`                                   |
| `DateTimeUtc(Year,Month,Day,Hour,Minute,Second)`               | Creates a Date and Time value in Universal Time Coordinates.                                                                           | Extracts year, month, day, hour, minute and second from a UTC date & time value.                | `DateTimeUtc(2016,03,05,19,17,23)`                          |
| `DateTimeUtc(Year,Month,Day,Hour,Minute,Second,MSecond)`       | Creates a Date and Time value in Universal Time Coordinates.                                                                           | Extracts year, month, day, hour, minute, second and millisecond from a UTC date & time value.   | `DateTimeUtc(2016,03,05,19,17,23,123)`                      |
| `Day[s](TP)`                                                   | Returns the day of a Date and Time value, or the total number days from a TimeSpan, or the days component from a Duration.             |                                                                                                 | `Day(Now)`                                                  |
| `Hour[s](TP)`                                                  | Returns the hour of a Date and Time value, or the total number of hours from a TimeSpan, or the hours component from a Duration.       |                                                                                                 | `Hours(Now)`                                                |
| `Local(TP)`                                                    | Converts a date and time value to local time coordinates.                                                                              |                                                                                                 | `Local(Now)`                                                |
| `Minute[s](TP)`                                                | Returns the minute of a Date and Time value, or the total number of minutes from a TimeSpan, or the minutes component from a Duration. |                                                                                                 | `Minutes(Now)`                                              |
| `Month[s](TP)`                                                 | Returns the month of a Date and Time value, or the months component from a Duration.                                                   |                                                                                                 | `Month(Now)`                                                |
| `Second[s](TP)`                                                | Returns the second of a Date and Time value, or the total number of seconds from a TimeSpan, or the second component from a Duration.  |                                                                                                 | `Seconds(Now)`                                              |
| `TimeSpan(s)`                                                  | Parses a TimeSpan value from a string, or extracts the time component of a date and time value.                                        | Asserts `s` is a time span value.                                                               | `TimeSpan("14:34:12")`                                      |
| `TimeSpan(Hour,Minute,Second)`                                 | Creates a TimeSpan value.                                                                                                              | Extracts hours, minutes and seconds from a time span value.                                     | `TimeSpan(14,34,12)`                                        |
| `TimeSpan(Day,Hour,Minute,Second)`                             | Creates a TimeSpan value.                                                                                                              | Extracts days, hours, minutes and seconds from a time span value.                               | `TimeSpan(2,14,34,12)`                                      |
| `TimeSpan(Day,Hour,Minute,Second,MSecond)`                     | Creates a TimeSpan value.                                                                                                              | Extracts days, hours, minutes, seconds and milliseconds from a time span value.                 | `TimeSpan(2,14,34,12,123)`                                  |
| `Utc(TP)`                                                      | Converts a date and time value to universal time coordinates (UTC).                                                                    |                                                                                                 | `Utc(Now)`                                                  |
| `Year[s](TP)`                                                  | Returns the year of a Date and Time value, or the years component from a Duration.                                                     |                                                                                                 | `Year(Now)`                                                 |

**Note**: When specifying a date and time using ticks, the number of ticks can either
mean the number of seconds from the UNIX Epoch (at 1970-01-01 00:00:00), if ticks
fit as a 32-bit integer, or the number of 100ns ticks since 0001-01-01 00:00:00.

### Vector Functions

The following functions operate on vectors:

| Function                                               | Description | Example |
|--------------------------------------------------------|-------------|---------|
| `And(v)`                                               | Logical or binary AND of all elements in vector | `And([1,2,3,4,5])`, `And([true,false,true])` |
| `Avg(v)`                                               | Alias for `Average(v)` | `Avg([1,2,3,4,5])` |
| `Average(v)`                                           | Average of elements in the vector `v`. | `Average([1,2,3,4,5])` |
| `Contains(v,x)`                                        | Returns `true` if `v` contains `x` as an element, `false` otherwise. | `Contains(v,1)` |
| `Count(v)`                                             | Number of elements in the vector `v`. | `Count([1,2,3,4,5])` |
| `Count(v,x)`                                           | Number of elements in the vector `v` that are equal to `x`. | `Count([1,2,3,2,1],2)` |
| `FindElements(Search,V)`                               | Finds elements in a vector, and returns a vector of N elements representing the indices, where N is the number of elements found. | `FindElements("Hello",V)` |
| `First(v)`                                             | Returns the first element in a vector `v`. | `First([1,2,3,4,5])` |
| `IndexOf(v,x[,From])`                                  | Returns the zero-based index of `x` in `v`. If `x` is not found in `v`, `-1` is returned. If `From` is provided, search is started from this index. | `IndexOf(v,1)` |
| `Join(v)`                                              | Joins a vector of vectors, into a larger vector. | `Join(v)` |
| `Join(v1,v2[,v3[,v4[,v5[,v6[,v7[,v8[,v9]]]]]]])`       | Joins a sequence of vectors, into a larger vector. | `Join(v1,v2)` |
| `Last(v)`                                              | Returns the last element in a vector `v`. | `Last([1,2,3,4,5])` |
| `LastIndexOf(v,x[,From])`                              | Returns the last zero-based index of `x` in `v`. If `x` is not found in `v`, `-1` is returned. If `From` is provided, search is started from this index. | `LastIndexOf(v,1)` |
| `Left(v,N)`                                            | Returns a vector with the left-most `N` elements. If the vector `v` is shorter, the entire vector is returned. | `Left(v,3)` |
| `Max(v)`                                               | The largest element in the vector `v`. | `Max([1,2,3,4,5])` |
| `Median(v)`                                            | The median element in the vector `v`. | `Median([1,2,3,4,5])` |
| `Mid(v,Pos,Len)`                                       | Returns a vector containing elements from `v`, starting a element `Pos` and continuing `Len` elements. The `Pos` index is zero-based. If the requested vector goes beyond the scope of `v`, the resulting vector gets truncated accordingly. | `Mid(v,5,2)` |
| `Min(v)`                                               | The smallest element in the vector `v`. | `Min([1,2,3,4,5])` |
| `Nand(v)`                                              | Logical or binary NAND of all elements in vector | `Nand([1,2,3,4,5])`, `Nand([true,false,true])` |
| `Nor(v)`                                               | Logical or binary NOR of all elements in vector | `Nor([1,2,3,4,5])`, `Nor([true,false,true])` |
| `Ones(N)`                                              | Creates an N-dimensional vector with all elements set to 1. | `Ones(5)` |
| `Or(v)`                                                | Logical or binary OR of all elements in vector | `Or([1,2,3,4,5])`, `Or([true,false,true])` |
| `PopFirst(v)`                                          | Returns the first element in a vector `v`, and if `v` is a variable reference, the variable will contain a vector where the first element has been removed. | `PopFirst(v)` |
| `PopLast(v)`                                           | Returns the last element in a vector `v`, and if `v` is a variable reference, the variable will contain a vector where the last element has been removed. | `PopLast(v)` |
| `Prod(v)`                                              | Alias for `Product(v)` | `Prod([1,2,3,4,5])` |
| `Product(v)`                                           | Product of elements in the vector `v`. | `Product([1,2,3,4,5])` |
| `PushFirst(v)`                                         | Adds a new element to a vector `v`, increasing its dimension, adding the element as the first element. If `v` is a variable reference, the variable will contain the new vector. | `PushFirst(x,v)` |
| `PushLast(v)`                                          | Adds a new element to a vector `v`, increasing its dimension, adding the element as the last element. If `v` is a variable reference, the variable will contain the new vector. | `PushLast(x,v)` |
| `Reverse(s)`                                           | Returns a string with the characters of the string `s` reversed. | `Reverse("Hello World")` |
| `Reverse(v)`                                           | Returns a vector with the elements of the original vector `v` in reverse order. | `Reverse([1,2,3,4,5])` |
| `Right(v,N)`                                           | Returns a vector with the right-most `N` elements. If the vector `v` is shorter, the entire vector is returned. | `Right(v,3)` |
| `Sample(v)`                                            | Returns a ramdnom sample element from the vector. | `Sample(1..10)` |
| `Sort(v[,x1[,x2][,x3][,x4][,x5][,x6][,x7][,x8][,x9]])` | Sorts a vector `v`. `x1`-`x9` are optional, and can be index values, field names or lambda expressions, and determine how to sort the vector `v`. Negative index numbers, or property of field names beginning with a hyphen `-` are sorted in decending order. Index numbers are one-based, as opposed to normal index values that are zero-based. | `Sort(v,"Field")` |
| `StdDev(v)`                                            | Alias for `StandardDeviation(v)`  | `StdDev([1,2,3,4,5])` |
| `StandardDeviation(v)`                                 | Standard deviation of elements in the vector `v`. | `StandardDeviation([1,2,3,4,5])` |
| `Sum(v)`                                               | Sum of elements in the vector `v`. | `Sum([1,2,3,4,5])` |
| `Var(v)`                                               | Alias for `Variance(v)` | `Var([1,2,3,4,5])` |
| `Variance(v)`                                          | Variance of elements in the vector `v`. | `Variance([1,2,3,4,5])` |
| `Xnor(v)`                                              | Logical or binary XNOR of all elements in vector | `Xnor([1,2,3,4,5])`, `Xnor([true,false,true])` |
| `Xor(v)`                                               | Logical or binary XOR of all elements in vector | `Xor([1,2,3,4,5])`, `Xor([true,false,true])` |
| `Zeroes(N)`                                            | Creates an N-dimensional vector with all elements set to 0. | `Zeroes(5)` |

### Matrix Functions

The following functions operate on matrices:

| Function                   | Description                                                                                                                                                                                                                                                         | Example                    |
|----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| `Columns(M)`               | Returns the number of columns in the matrix `M`. (From the Waher.Script.Graphs3D extension.)                                                                                                                                                                        | `Columns(Identity(3))=3`   |
| `Columns(v[,SecondDim])`   | Creates a matrix whose columns have elements of the same value, each defined by the corresponding element in the input vector.                                                                                                                                      | `Columns(X)`               |
| `Determinant(M)`           | Returns the determinant of a matrix `M`.                                                                                                                                                                                                                            | `Determinant(Identity(3))` |
| `Det(M)`                   | Alias for `Determinant(M)`.                                                                                                                                                                                                                                         | `det(Identity(3))`         |
| `Diagonal(M)`              | Returns the diagonal vector of a matrix `M`.                                                                                                                                                                                                                        | `Diagonal(Identity(3))`    |
| `Diag(M)`                  | Alias for `Diagonal(M)`.                                                                                                                                                                                                                                            | `diag(Identity(3))`        |
| `Eliminate(M)`             | Reduces the rows in the matrix `M`, and eliminates corresponding elements. (Also called Gauss-Jordan elimination of the matrix.)                                                                                                                                    | `Eliminate(M)`             |
| `FindElements(Search,M)`   | Finds elements in a matrix, and returns a matrix of two columns and N rows representing the coordinates, where N is the number of elements found.                                                                                                                   | `FindElements("Hello",M)`  |
| `Identity(N)`              | Creates an NxN identity matrix.                                                                                                                                                                                                                                     | `Identity(10)`             |
| `IndexOf(M,x[,FC,FR])`     | Returns the zero-based column and row index of the element `x` in `M`. If `x` is not found in `M`, `[-1,-1]` is returned. If `FC` and `FR` is provided, search is started from this the `FC` column and `FR` row. Search is done left to right, top to bottom.      | `IndexOf(M,1)`             |
| `Inv(M)`                   | Alias for `Invert(M)`.                                                                                                                                                                                                                                              | `Inv([[1,1],[0,1]])`       |
| `Inverse(M)`               | Alias for `Invert(M)`.                                                                                                                                                                                                                                              | `Inverse([[1,1],[0,1]])`   |
| `Invert(M)`                | Inverts `M`. Works on any invertable element.                                                                                                                                                                                                                       | `Invert([[1,1],[0,1]])`    |
| `LastIndexOf(M,x[,FC,FR])` | Returns the last zero-based column and row index of the element `x` in `M`. If `x` is not found in `M`, `[-1,-1]` is returned. If `FC` and `FR` is provided, search is started from this the `FC` column and `FR` row. Search is done right to left, bottom to top. | `LastIndexOf(M,1)`         |
| `Ones(Rows,Columns)`       | Creates an MxN-matrix with all elements set to 1.                                                                                                                                                                                                                   | `Ones(5,4)`                |
| `Rank(M)`                  | Computes the rank of the matrix `M`.                                                                                                                                                                                                                                | `Rank(M)=Rows(M)`          |
| `Reduce(M)`                | Reduces the rows in the matrix `M`. (Also called Gauss-elimination, or row reduction of a matrix.)                                                                                                                                                                  | `Reduce(M)`                |
| `Rows(M)`                  | Returns the number of rows in the matrix `M`. (From the Waher.Script.Graphs3D extension.)                                                                                                                                                                           | `Rows(Identity(3))=3`      |
| `Rows(v[,SecondDim])`      | Creates a matrix whose rows have elements of the same value, each defined by the corresponding element in the input vector.                                                                                                                                         | `Rows(Z)`                  |
| `Trace(M)`                 | Returns the sum of the elements on the diagonal of a matrix `M`.                                                                                                                                                                                                    | `Trace(Identity(3))`       |
| `Tr(M)`                    | Alias for `Trace(M)`.                                                                                                                                                                                                                                               | `tr(Identity(3))`          |
| `Zeroes(Rows,Columns)`     | Creates an MxN-matrix with all elements set to 0.                                                                                                                                                                                                                   | `Zeroes(5,4)`              |

**Note**: The transpose and conjugate transpose of a matrix is performed using the `T` and `H`
suffix operators, not by function calls.

### Transforms

The following functions generate transformation matrices:

| Function | Description | Example |
|----------|-------------|---------|
| `Rotate2D(rad)` | Generates a rotation matrix in two-dimensional space. `rad` is given in radians. The ° operator can be used to convert degrees to radians. | `Rotate2D(45°)` |
| `Rotate2DH(rad)` | Generates a rotation matrix in two-dimensional space using homogeneous coordinates. `rad` is given in radians. The ° operator can be used to convert degrees to radians. | `Rotate2DH(45°)` |
| `Scale2D(sx,sy)` | Generates a scaling matrix in two-dimensional space. | `Scale2D(0.5,2)` |
| `Scale2DH(sx,sy)` | Generates a scaling matrix in two-dimensional space using homogeneous coordinates. | `Scale2DH(0.5,2)` |
| `Translate2DH(dx,dy)` | Generates a translation matrix in two-dimensional space using homogeneous coordinates. | `Translate2DH(10,-20)` |

### Runtime Functions

The following functions are useful to control the runtime execution of the script:

| Function                      | Description                                                                                                                                                                                                                                | In Pattern Matching                                    | Example |
|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------|---------|
| `Break([x])`                  | Breaks the current loop. If a value is included as an argument, it is taken as the final value of the loop.                                                                                                                                |                                                        | `break(Result)` |
| `Continue([x])`               | Skips the rest of the content evaluation and continues to the condition statement of the loop. If a value is included as an argument, it is taken as the final value of the current iteration, otherwise the interation lacks a value.     |                                                        | `continue(Result)` |
| `Create(Type[,ArgList])`      | Creates an object instance of type `Type`. `ArgList` contains an optional list of arguments. If `Type` is a generic type, the generic type arguments precede any constructor arguments.                                                    |                                                        | `Create(System.String,'-',80)` |
| `CreateType(Type[,TypeList])` | Creates a concrete type from a generic type `Type` together with a list of types in `TypeList` forming the basis for the concretization of the generic type.                                                                               |                                                        | `CreateType(System.Array,System.Byte)` |
| `Delete(x)`                   | Alias for `Destroy(x)`.                                                                                                                                                                                                                    |                                                        | `Delete(x)` |
| `Destroy(x)`                  | Destroys the value `x`. If the function references a variable, the variable is also removed.                                                                                                                                               |                                                        | `Destroy(x)` |
| `Error(Msg)`                  | Throws an error/exception.                                                                                                                                                                                                                 |                                                        | `Error('Something went wrong.')` |
| `Exception(Msg)`              | Alias for `Error(Msg)`.                                                                                                                                                                                                                    |                                                        | `Exception('Something went wrong.')` |
| `Exists(f)`                   | Checks if the expression defined by `f` is valid or not.                                                                                                                                                                                   |                                                        | `Exists(x)` |
| `Fields(x)`                   | If `x` is a type, `Fields(x)` returns a vector of field names. If `x` is not a type, `Fields(x)` returns a matrix containing field names and values.                                                                                       |                                                        | `Properties(Ans)` |
| `Methods(x)`                  | If `x` is a type, `Methods(x)` returns a vector of methods represented as strings. If `x` is not a type, `Methods(x)` returns a matrix containing method names and lambda functions that can be used to execute the corresponding methods. |                                                        | `Methods(Ans)` |
| `Names(x)`                    | If `x` is an enumeration type or value, `Names(x)` returns the list of recognized enumeration value of the corresponding enumeration type.                                                                                                 |                                                        | `Names(Ans)` |
| `Optional(f)`                 | Declares `f` as optional. Useful in pattern matching.                                                                                                                                                                                      | Any variables in `f` are assigned null if not defined. | `Optional(x)` |
| `Preview(x)`                  | Reports a preview of the result back to the caller, if subscribed for such.                                                                                                                                                                |                                                        | `Preview(x)` |
| `Print(Msg)`                  | Prints a message to the current console output (which is defined in the variables collection).                                                                                                                                             |                                                        | `Print(x)` |
| `PrintLine([Msg])`            | Prints a message followed by a newline to the current console output.                                                                                                                                                                      |                                                        | `PrintLine(x)` |
| `PrintLn([Msg])`              | Alias for `PrintLine(Msg)`.                                                                                                                                                                                                                |                                                        | `PrintLine(x)` |
| `Properties(x)`               | If `x` is a type, `Properties(x)` returns a vector of property names. If `x` is not a type, `Properties(x)` returns a matrix containing property names and values.                                                                         |                                                        | `Properties(Ans)` |
| `Remove(Var)`                 | Removes the varable `Var` without destroying its contents.                                                                                                                                                                                 |                                                        | `Remove(x)` |
| `Remove(Obj.Property)`        | Removes the property `Property` from an object, without destroying its contents. Returns if the named property was found and removed.                                                                                                      |                                                        | `Remove(x.A)` |
| `Required(f)`                 | Makes sure `f` is defined. If not, an exception is thrown. Can be used in pattern matching.                                                                                                                                                | Asserts `f` is defined (and not null).                 | `Required(x)` |
| `Return(x)`                   | Returns from the current function scope with the value `x`.                                                                                                                                                                                |                                                        | `return(Result)` |

### Logging Functions

The following functions can be used to log information to the event log:

| Function                           | Description                                   | Example                                      |
|------------------------------------|-----------------------------------------------|----------------------------------------------|
| `LogInformational(Message[,Tags])` | Logs an informational event to the event log. | `LogInformational("Hello",{Actor:"Kilroy"})` |
| `LogInformation(Message[,Tags])`   | Same as `LogInformational`.                   | `LogInformation("Hello",{Actor:"Kilroy"})`   |
| `LogInfo(Message[,Tags])`          | Same as `LogInformational`.                   | `LogInfo("Hello",{Actor:"Kilroy"})`          |
| `LogNotice(Message[,Tags])`        | Logs a notice event to the event log.         | `LogNotice("Hello",{Actor:"Kilroy"})`        |
| `LogWarning(Message[,Tags])`       | Logs a warning event to the event log.        | `LogWarning("Hello",{Actor:"Kilroy"})`       |
| `LogError(Message[,Tags])`         | Logs an error event to the event log.         | `LogError("Hello",{Actor:"Kilroy"})`         |
| `LogCritical(Message[,Tags])`      | Logs a critical event to the event log.       | `LogCritical("Hello",{Actor:"Kilroy"})`      |
| `LogAlert(Message[,Tags])`         | Logs an alert event to the event log.         | `LogAlert("Hello",{Actor:"Kilroy"})`         |
| `LogEmergency(Message[,Tags])`     | Logs an emergency event to the event log.     | `LogEmergency("Hello",{Actor:"Kilroy"})`     |
| `LogDebug(Message[,Tags])`         | Logs a debug event to the event log.          | `LogDebug("Hello",{Actor:"Kilroy"})`         |

**Note**: The `Message` can be a string message, or an object derived from `Exception`. If such an object any of the following
interfaces (from the `Waher.Events` namespace), the corresponding attributes will be logged with the message automatically:
`IEventObject`, `IEventActor`, `IEventId`, `IEventLevel`, `IEventFacility`, `IEventModule` and `IEventTags`.

**Note 2**: The `Tags` argument in the logging functions should be an object-ex-nihilo (or similar object), providing tags as key-value
pairs. Any tag name can be used, however, the Event log treats the following tag names specially: `Object`, `Actor`, `EventId`, `Level`, 
`Facility`, `Module`, `StackTrace`.

### Extensions

The script engine can be extended by modules that are run in the environment. The following subssections list such funcion extensions
made available in different modules available by default on the gateway. This list does not include funcion extensions made available
by applications that are not part of **{{Waher.IoTGateway.Gateway.ApplicationName}}**.

#### Color functions (Waher.Script.Graphs)

The following functions are available in the `Waher.Script.Graphs` library.

| Function                  | Description | Example |
|---------------------------|-------------|---------|
| `Alpha(Color,Alpha)`      | Sets the Alpha channel of a color.                                                                                                                                      | `Alpha("Red",128)` |
| `Blend(c1,c2,p)`          | Blends colors `c1` and `c2` together using a blending factor 0<=`p`<=1. Any or both of `c1` and `c2` can be an image.                                                   | `Blend("Red","Green",0.5)` |
| `Color(string)`           | Parses a string and returns the corresponding color. The color can either be a known color name, or in any of the formats `RRGGBB`, `RRGGBBAA`, `#RRGGBB`, `#RRGGBBAA`. | `Color("Red")`        |
| `ColorGradient(colors,p)` | Returns a color from a color gradient defined by a vector of colors and an interpolation constant 0<=`p`<=1.                                                            | `ColorGradient(["Green","Yellow","Red"],0.75)` |
| `GrayScale(Color)`        | Converts a color to its corresponding Gray-scale value.                                                                                                                 | `GrayScale(cl)`        |
| `HSL(H,S,L)`              | Creates a color from its HSL representation.                                                                                                                            | `HSL(100,0.5,0.7)`     |
| `HSLA(H,S,L,A)`           | Creates a color from its HSLA representation.                                                                                                                           | `HSLA(100,0.5,0.7,64)` |
| `HSV(H,S,V)`              | Creates a color from its HSV representation.                                                                                                                            | `HSV(100,0.5,0.7)`     |
| `HSVA(H,S,V,A)`           | Creates a color from its HSVA representation.                                                                                                                           | `HSVA(100,0.5,0.7,64)` |
| `RGB(R,G,B)`              | Creates a color from its RGB representation.                                                                                                                            | `RGB(100,150,200)`     |
| `RGBA(R,G,B,A)`           | Creates a color from its RGBA representation.                                                                                                                           | `RGBA(100,150,200,64)` |

**Note**: In all functions expecting color arguments, you can pass any of the [named colors](ScriptColors.md) as a string instead.
The named colors will be recognized, and converted to the corresponding color.

#### Graph functions (Waher.Script.Graphs)

The following functions are available in the `Waher.Script.Graphs` library. In an interactive script environment, clicking on the resulting graphs
will return a vector corresponding to the point under the mouse.

| Function                                           | Description                                           | Example                                   |
|----------------------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `Canvas(Width,Height[Color[,BgColor]])`            | Creates a 2D canvas for custom drawing.               | [Example](CanvasExample)                  |
| `HorizontalBars(Labels,Values[,Color])`            | Plots a two-dimensional stacked horizontal bar chart. | [Example][HorizontalBarsExample]          |
| `Plot2DArea(X,Y[,Color])`                          | Plots a stacked area chart.                           | [Example][Plot2DAreaExample]              |
| `Plot2DCurve(X,Y[,Color[,PenSize]])`               | Plots a smooth two-dimensional curve.                 | [Example][Plot2DCurveExample]             |
| `Plot2DCurveArea(X,Y[,Color])`                     | Plots a stacked spline area chart.                    | [Example][Plot2DCurveAreaExample]         |
| `Plot2DH(X,Y,Mode[,Color[,PenSize]])`              | Alias for `Plot2dHorizontalLine`.                     | [Example][Plot2DHLineExample]             |
| `Plot2DHorizontalLine(X,Y,Mode[,Color[,PenSize]])` | Plots a two-dimensional horizontal line graph.        | [Example][Plot2DHorizontalLineExample]    |
| `Plot2DLayeredArea(X,Y[,Color])`                   | Plots a layered area chart.                           | [Example][Plot2DLayeredAreaExample]       |
| `Plot2DLayeredCurveArea(X,Y[,Color])`              | Plots a layered spline area chart.                    | [Example][Plot2DLayeredCurveAreaExample]  |
| `Plot2DLayeredLineArea(X,Y[,Color])`               | Alias for `Plot2DLayeredArea`.                        | [Example][Plot2DLayeredLineAreaExample]   |
| `Plot2DLayeredSplineArea(X,Y[,Color])`             | Alias for `Plot2DLayeredCurveArea`.                   | [Example][Plot2DLayeredSplineAreaExample] |
| `Plot2DLine(X,Y[,Color[,PenSize]])`                | Plots a two-dimensional line graph.                   | [Example][Plot2DLineExample]              |
| `Plot2DLineArea(X,Y[,Color])`                      | Alias for `Plot2DArea`.                               | [Example][Plot2DLineAreaExample]          |
| `Plot2DSpline(X,Y[,Color[,PenSize]])`              | Plots a smooth two-dimensional curve.                 | [Example][Plot2DSplineExample]            |
| `Plot2DSplineArea(X,Y[,Color])`                    | Alias for `Plot2DCurveArea`.                          | [Example][Plot2DSplineAreaExample]        |
| `Polygon2D(X,Y[,Color])`                           | Plots a filled polygon.                               | [Example][Polygon2DExample]               |
| `SameScale(Graph)`                                 | Informs the graph to use the same scale for all axes. | [Example][SameScaleExample]               |
| `Scatter2D(X,Y[,Color[,BulletSize]])`              | Plots a two-dimensional scatter diagram.              | [Example][Scatter2DExample]               |
| `VerticalBars(Labels,Values[,Color])`              | Plots a two-dimensional stacked vertical bar chart.   | [Example][VerticalBarsExample]            |

[CanvasExample]: Prompt.md?Expression=Canvas%28500,500,"Red","White"%29
[HorizontalBarsExample]: Prompt.md?Expression=x%3A%3D0..20%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3BHorizontalBars(%22x%22%2Bx%2Cy%2Crgba(255%2C0%2C0%2C128))%2BHorizontalBars(%22x%22%2Bx%2Cy2%2Crgba(0%2C0%2C255%2C128))%3B
[Plot2DAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3Bplot2darea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2darea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dline(x%2Cy)%2Bplot2dline(x%2Cy2%2C%22Blue%22)
[Plot2DCurveExample]: Prompt.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0aplot2dcurve(x,y)
[Plot2DCurveAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3Bplot2dcurvearea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dcurvearea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dcurve(x%2Cy)%2Bplot2dcurve(x%2Cy2%2C%22Blue%22)%2Bscatter2d(x%2Cy%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy2%2C%22Blue%22%2C5)
[Plot2DHLineExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3Bplot2dhline(x%2Cy%2C0%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy%2C%22Blue%22%2C5)%2Bplot2dline(x%2Cy%2C%22Blue%22%2C1)%3B
[Plot2DHorizontalLineExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3Bplot2dHorizontalLine(x%2Cy%2C0%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy%2C%22Blue%22%2C5)%2Bplot2dline(x%2Cy%2C%22Blue%22%2C1)%3B
[Plot2DLayeredAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x%2F2)%3Bplot2dlayeredarea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dlayeredarea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dline(x%2Cy)%2Bplot2dline(x%2Cy2%2C%22Blue%22)
[Plot2DLayeredCurveAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x%2F2)%3Bplot2dlayeredcurvearea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dlayeredcurvearea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dcurve(x%2Cy)%2Bplot2dcurve(x%2Cy2%2C%22Blue%22)%2Bscatter2d(x%2Cy%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy2%2C%22Blue%22%2C5)
[Plot2DLayeredLineAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x%2F2)%3Bplot2dlayeredlinearea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dlayeredlinearea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dline(x%2Cy)%2Bplot2dline(x%2Cy2%2C%22Blue%22)
[Plot2DLayeredsplineAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x%2F2)%3Bplot2dlayeredsplinearea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dlayeredsplinearea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dspline(x%2Cy)%2Bplot2dspline(x%2Cy2%2C%22Blue%22)%2Bscatter2d(x%2Cy%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy2%2C%22Blue%22%2C5)
[Plot2DLineExample]: Prompt.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0aplot2dline(x,y)
[Plot2DLineAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3Bplot2dlinearea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dlinearea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dline(x%2Cy)%2Bplot2dline(x%2Cy2%2C%22Blue%22)
[Plot2DSplineExample]: Prompt.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0aplot2dspline(x,y)
[Plot2DSplineAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3Bplot2dcurve(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dcurve(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dcurve(x%2Cy)%2Bplot2dcurve(x%2Cy2%2C%22Blue%22)%2Bscatter2d(x%2Cy%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy2%2C%22Blue%22%2C5)
[Polygon2DExample]: Prompt.md?Expression=t%3A%3D0..9%3Bx%3A%3Dsin(t*pi%2F5)%3By%3A%3Dcos(t*pi%2F5)%3Bpolygon2d(x%2Cy)%0A%0A
[Scatter2DExample]: Prompt.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0ascatter2d(x,y)
[VerticalBarsExample]: Prompt.md?Expression=x%3A%3D0..20%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3BVerticalBars(%22x%22%2Bx%2Cy%2Crgba(255%2C0%2C0%2C128))%2BVerticalBars(%22x%22%2Bx%2Cy2%2Crgba(0%2C0%2C255%2C128))%3B
[SameScaleExample]: Prompt.md?Expression=SameScale%28plot2dcurve%28x%2Cy%29%29

The following table lists variables that control graph output:

| Varaible             | Type    | Description                      | Current value                   |
|----------------------|---------|----------------------------------|---------------------------------|
| `GraphWidth`         | Double  | Width of graph, in pixels.       | `{{GraphWidth ??? ""}}`         |
| `GraphHeight`        | Double  | Height of graph, in pixels.      | `{{GraphHeight ??? ""}}`        |
| `GraphBgColor`       | Color   | Background color.                | `{{GraphBgColor ??? ""}}`       |
| `GraphFgColor`       | Color   | Foreground color.                | `{{GraphFgColor ??? ""}}`       |
| `GraphLabelFontSize` | Double  | Label font size.                 | `{{GraphLabelFontSize ??? ""}}` |


You can combine graphs using the `+` operator, as long as graph axes are compatible:

	x:=-10..10;
	y:=sin(x);
	y2:=2*sin(x);
	plot2dcurvearea(x,y,rgba(255,0,0,64))+
	   plot2dcurvearea(x,y2,rgba(0,0,255,64))+
	   plot2dcurve(x,y)+
	   plot2dcurve(x,y2,"Blue")+
	   scatter2d(x,y,"Red",5)+
	   scatter2d(x,y2,"Blue",5)

{
GraphWidthBak:=try GraphWidth catch 640;
GraphHeightBak:=try GraphHeight catch 480;
x:=-10..10;
y:=sin(x);
y2:=2*sin(x);
plot2dcurvearea(x,y,rgba(255,0,0,64))+
   plot2dcurvearea(x,y2,rgba(0,0,255,64))+
   plot2dcurve(x,y)+
   plot2dcurve(x,y2,"Blue")+
   scatter2d(x,y,"Red",5)+
   scatter2d(x,y2,"Blue",5)
}

Graph-types having string labels on one axis, and values on another, can be added using element-wise addition operator `.+` as well. 
When this operator is used, not only are the graphs added, but the internal values are accumulated as well. Example:

	Labels:=["A","B","C","D","E","F"];
	Colors:=["Red","Green","Blue","Yellow","Magenta","LightGray"];
	G:=[foreach x in 0..5 do VerticalBars(Labels,Uniform(0,10,6),Alpha(Colors[x],192))];
	G[0].+G[1].+G[2].+G[3].+G[4].+G[5];

{
Labels:=["A","B","C","D","E","F"];
Colors:=["Red","Green","Blue","Yellow","Magenta","LightGray"];
G:=[foreach x in 0..5 do VerticalBars(Labels,Uniform(0,10,6),Alpha(Colors[x],192))];
G[0].+G[1].+G[2].+G[3].+G[4].+G[5];
}

Normal addition, using the same values is achieved easilly as follows. The half-transparent bars are painted one on-top of the other,
as they occupy the same numerical space.

	Sum(G)

{
	Sum(G)
}

Use the `GraphWidth` and `GraphHeight` variables to control graph output size. The following example shows
how to construct a [Sparkline](https://en.wikipedia.org/wiki/Sparkline) graph:

	x:=0..100;
	y0:=0;
	y:=[foreach i in x do y0:=y0+Uniform(-1,1)];
	GraphWidth:=200;
	GraphHeight:=25;
	Sparkline:=plot2dline(x,y,"Black",1)+scatter2d(100,y[y.Length-1],"Red",2);
	Sparkline.ShowXAxis:=false;
	Sparkline.ShowYAxis:=false;
	Sparkline.ShowGrid:=false;
	Sparkline;

{
x:=0..100;
y0:=0;
y:=[foreach i in x do y0:=y0+Uniform(-1,1)];
GraphWidth:=200;
GraphHeight:=25;
Sparkline:=plot2dline(x,y,"Black",1)+scatter2d(100,y[y.Length-1],"Red",2);
Sparkline.ShowXAxis:=false;
Sparkline.ShowYAxis:=false;
Sparkline.ShowGrid:=false;
Sparkline;
}

If you use layered graphs that are painted ontop of underlying graphs, you can use the alpha channel to add transparency:

	x:=-10..10;
	y:=sin(x);
	y2:=2*sin(x/2);
	plot2dlayeredcurvearea(x,y,rgba(255,0,0,64))+
	   plot2dlayeredcurvearea(x,y2,rgba(0,0,255,64))+
	   plot2dcurve(x,y)+
	   plot2dcurve(x,y2,"Blue")+
	   scatter2d(x,y,"Red",5)+
	   scatter2d(x,y2,"Blue",5)

{
GraphWidth:=GraphWidthBak;
GraphHeight:=GraphHeightBak;
x:=-10..10;
y:=sin(x);
y2:=2*sin(x/2);
plot2dlayeredcurvearea(x,y,rgba(255,0,0,64))+
   plot2dlayeredcurvearea(x,y2,rgba(0,0,255,64))+
   plot2dcurve(x,y)+
   plot2dcurve(x,y2,"Blue")+
   scatter2d(x,y,"Red",5)+
   scatter2d(x,y2,"Blue",5)
}

You can set the properties `Title`, `LabelX` and `LabelY` to descriptive strings, to provide information to the reader:

	GraphWidth:=800;
	GraphHeight:=400;
	f:=x->sin(5*x)*exp(-(x^2/10));
	x:=-10..10|0.1;
	G:=plot2dcurve(x,f(x),"Blue")+plot2dcurve(x,f'(6)*(x-6)+f(6))+plot2dcurve(x,f'(-6)*(x+6)+f(-6))+scatter2d([-6,6],[f(-6),f(6)],"Red");
	G.Title:="Tangent at x=6 and x=-6";
	G.LabelX:="x";
	G.LabelY:="y=sin(5x)exp(-(x^2/10))";
	G

{
GraphWidth:=800;
GraphHeight:=400;
f:=x->sin(5*x)*exp(-(x^2/10));
x:=-10..10|0.1;
G:=plot2dcurve(x,f(x),"Blue")+plot2dcurve(x,f'(6)*(x-6)+f(6))+plot2dcurve(x,f'(-6)*(x+6)+f(-6))+scatter2d([-6,6],[f(-6),f(6)],"Red");
G.Title:="Tangent at x=6 and x=-6";
G.LabelX:="x";
G.LabelY:="y=sin(5x)exp(-(x^2/10))";
G
}

#### 3D Graph functions (Waher.Script.Graphs3D)

The following graph functions are available in the `Waher.Script.Graphs3D` library.

| Function                                   | Description                                           | Example                                   |
|--------------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `LineMesh3D(X,Y,Z[,Color])`                | Draws a three dimensional line mesh from coordinates in three equally sized matrices `X`, `Y`, `Z`.    | [Example][LineMesh3DExample] |
| `PolygonMesh3D(X,Y,Z[,Shader[,TwoSided]])` | Draws a three dimensional polygon mesh from coordinates in three equally sized matrices `X`, `Y`, `Z`. | [Example][PolygonMesh3DExample] |
| `Surface3D(X,Y,Z[,Shader[,TwoSided]])`     | Draws a three dimensional surface from coordinates in three equally sized matrices `X`, `Y`, `Z`.      | [Example][Surface3DExample] |
| `VerticalBars3D(X,Y,Z[,Shader])`           | Draws a three dimensional vertical bar chart from information available in three equally sized matrices `X`, `Y`, `Z`. `X` and `Z` are assumed to be labels. `Y` contains the corresponding bar value. | [Example][VerticalBars3DExample] |

[LineMesh3DExample]: Prompt.md?Expression=x%3A%3DColumns%28-10..10%7C0.5%29%3Bz%3A%3DRows%28-10..10%7C0.5%29%3Br%3A%3Dsqrt%28x.%5E2%2Bz.%5E2%29%3By%3A%3Dsin%28r%2A2%29.%2Aexp%28-r%2F3%29%3Blinemesh3d%28x%2Cy%2Cz%2C%27Blue%27%29
[PolygonMesh3DExample]: Prompt.md?Expression=x%3A%3DColumns%28-10..10%7C0.5%29%3Bz%3A%3DRows%28-10..10%7C0.5%29%3Br%3A%3Dsqrt%28x.%5E2%2Bz.%5E2%29%3By%3A%3Dsin%28r%2A2%29.%2Aexp%28-r%2F3%29%3Bpolygonmesh3d%28x%2Cy%2Cz%2C%27Blue%27%29
[Surface3DExample]: Prompt.md?Expression=x%3A%3DColumns%28-10..10%7C0.5%29%3Bz%3A%3DRows%28-10..10%7C0.5%29%3Br%3A%3Dsqrt%28x.%5E2%2Bz.%5E2%29%3By%3A%3Dsin%28r%2A2%29.%2Aexp%28-r%2F3%29%3Bsurface3d%28x%2Cy%2Cz%2C%27Blue%27%29
[VerticalBars3DExample]: Prompt.md?Expression=%5BLabelsX%2CLabelsZ%2CY%5D%3A%3DHistogram2D%28%5BNormal%280%2C1%2C100000%29%2CNormal%280%2C1%2C100000%29%5D%2C-5%2C5%2C50%2C-5%2C5%2C50%29%3BVerticalBars3D%28Columns%28LabelsX%29%2CY%2CRows%28LabelsZ%29%29

To use the 3D graph functions, or the 3D graphic drawing primitives, you may also need to
use the following helper functions.

| Function                                              | Description                                           | Example                                   |
|-------------------------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `Canvas3D(Width,Height,Oversampling,BackgroundColor)` | Creates a 3D canvas for custom drawing.                                                                                               | [Example](Canvas3DExample) |
| `Columns(Values)`                                     | Creates a square matrix whose columns have elements of the same value, each defined by the corresponding element in the input vector. | [Example][ColumnsExample] |
| `Matrix4x4(m11,m12,m13,m14,m21,...,m44)`              | Creates a Matrix4x4 object (from the `System.Numerics` namespace).                                                                    | [Example][Matrix4x4Example] |
| `Rows(Values)`                                        | Creates a square matrix whose rows have elements of the same value, each defined by the corresponding element in the input vector.    | [Example][RowsExample] |
| `Vector3(X,Y,Z)`                                      | Creates a `Vector3` object (from the `System.Numerics` namespace).                                                                    | [Example][Vector3Example] |
| `Vector4(X,Y,Z,W)`                                    | Creates a `Vector4` object (from the `System.Numerics` namespace).                                                                    | [Example][Vector4Example] |

[Canvas3DExample]: Prompt.md?Expression=Canvas3D%28500,500,2,"White"%29
[ColumnsExample]: Prompt.md?Expression=X%3A%3DColumns%280..10%29
[Matrix4x4Example]: Prompt.md?Expression=Matrix4x4%281%2C0%2C0%2C0%2C0%2C1%2C0%2C0%2C0%2C0%2C1%2C0%2C0%2C0%2C0%2C1%29
[RowsExample]: Prompt.md?Expression=Z%3A%3DRows%280..10%29
[Vector3Example]: Prompt.md?Expression=Vector3%2810%2C20%2C30%29
[Vector4Example]: Prompt.md?Expression=Vector4%2810%2C20%2C30%2C1%29

Or the following shader-related functions:

| Function                                      | Description                                           | Example                                   |
|-----------------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `ConstantColor(Color)`                        | Creates a constant color shader from the color definition. | [Example][ConstantColorExample1] |
| `ConstantColor(Red,Green,Blue[,Alpha])`       | Creates a constant color shader from the `Red`, `Green` and `Blue` color components, and the optional `Alpha`. | [Example][ConstantColorExample2] |
| `PhongIntensity(Color)`                       | Creates a Phong Intensity definition from a color definition for use with a Phong Shader. | [Example][PhongIntensityExample1] |
| `PhongIntensity(Red,Green,Blue[,Alpha])`      | Creates a Phong Intensity definition from its color components and optional alpha component, for use with a Phong Shader. | [Example][PhongIntensityExample2] |
| `PhongLightSource(DI,SI,Position)`            | Creates a Light Source for use with a Phong Shader, providing a Diffuse Intensity `DI`, a Specular Intensity `SI` and a `Position` of the light source. | [Example][PhongLightSourceExample] |
| `PhongMaterial(AR,DR,SR,S)`                   | Provides material coefficients for a Phong Shader: An Ambient Reflection Constant in `AR`, a Diffuse Reflection Constant in `DR`, a Specular Reflection Constant in `SR` and a Shininess constant in `S`. | [Example][PhongMaterialExample] |
| `PhongShader(M,AI,Sources)`                   | Creates Phong Shader, by providing a Material definition in `M`, an Ambien Intensity in `AI` and a Light Source (or a vector of light sources) in `Sources`. | [Example][PhongShaderExample] |

[ConstantColorExample1]: Prompt.md?Expression=ConstantColor%28%22Red%22%29%0A
[ConstantColorExample2]: Prompt.md?Expression=ConstantColor%28255%2C0%2C0%2C64%29%0A
[PhongIntensityExample1]: Prompt.md?Expression=PhongIntensity%28%22Red%22%29%0A
[PhongIntensityExample2]: Prompt.md?Expression=PhongIntensity%28255%2C0%2C0%2C64%29%0A
[PhongLightSourceExample]: Prompt.md?Expression=PhongLightSource%28%22Red%22%2CPhongIntensity%28%22White%22%29%2C%5B1000%2C%201000%2C%200%5D%29
[PhongMaterialExample]: Prompt.md?Expression=PhongMaterial%281%2C%202%2C%200%2C%2010%29
[PhongShaderExample]: Prompt.md?Expression=PhongShader%28PhongMaterial%281%2C%202%2C%200%2C%2010%29%2CPhongIntensity%2864%2C%2064%2C%2064%2C%20255%29%2CPhongLightSource%28%22Red%22%2CPhongIntensity%28%22White%22%29%2C%5B1000%2C%201000%2C%200%5D%29%29

While the two-dimensional graph functions take two vectors as input, one for each of the
two coordinates (x and y), the three-dimensional graph functions take matrices as input, one 
for each of the three coordinates (x, y and z). To convert vectors to matrices suitable
for three-dimensional graphs, you can use the `Columns` and `Rows` functions respectively.
Since matrices are square, it is important to use the element-wise operators when using
arithmetic operators with the elements of the matrices. Otherwise, the corresponding matrix
operations will be used.

Example:

	x:=Columns(-10..10|0.1);
	z:=Rows(-10..10|0.1);
	r:=sqrt(x.^2+z.^2);
	y:=10*cos(r*2).*exp(-r/3);
	samescale(surface3d(x,y,z))

```async
GraphWidth:=640;
GraphHeight:=480;
x:=Columns(-10..10|0.1);
z:=Rows(-10..10|0.1);
r:=sqrt(x.^2+z.^2);
y:=10*cos(r*2).*exp(-r/3);
samescale(surface3d(x,y,z))
```

You can use the `Title`, `LabelX`, `LabelY`, `LabelZ`, `Angle`, `Inclination` and `Oversampling`
properties of the 3D-graph make the graph more informative:

Example:

	x:=Columns(-10..10|0.1);
	z:=Rows(-10..10|0.1);
	r:=sqrt(x.^2+z.^2);
	y:=10*cos(r*2).*exp(-r/3);
	G:=samescale(surface3d(x,y,z));
	G.Title:='Title';
	G.LabelX:='X-axis';
	G.LabelY:='Y-axis';
	G.LabelZ:='Z-axis';
	G.Angle:=45;
	G.Inclination:=60;
	G

```async
x:=Columns(-10..10|0.1);
z:=Rows(-10..10|0.1);
r:=sqrt(x.^2+z.^2);
y:=10*cos(r*2).*exp(-r/3);
G:=samescale(surface3d(x,y,z));
G.Title:='Title';
G.LabelX:='X-axis';
G.LabelY:='Y-axis';
G.LabelZ:='Z-axis';
G.Angle:=45;
G.Inclination:=60;
G
```

You can also use the addition operator, as with two-dimensional graphs, to show multiple graphs
simultanerously.

Example:

	Thorus(R0,R1,Color,dx,dy,dz):=
	(
		theta:=Columns((0..360|5)°);
		phi:=Rows((0..360|5)°);
		x:=(R1+R0*cos(theta)).*cos(phi)+dx;
		y:=R0*sin(theta)+dy;
		z:=(R1+R0*cos(theta)).*sin(phi)+dz;
		samescale(surface3d(x,y,z,Color))
	);

	Thorus(5,20,'Red',0,0,0)+
		Thorus(3,15,'Blue',0,10,0)+
		Thorus(2,10,'Green',0,17,0)+
		Thorus(1,7,'Yellow',0,20,0)

```async
Thorus(R0,R1,Color,dx,dy,dz):=
(
	theta:=Columns((0..360|5)°);
	phi:=Rows((0..360|5)°);
	x:=(R1+R0*cos(theta)).*cos(phi)+dx;
	y:=R0*sin(theta)+dy;
	z:=(R1+R0*cos(theta)).*sin(phi)+dz;
	samescale(surface3d(x,y,z,Color))
);

Thorus(5,20,'Red',0,0,0)+
	Thorus(3,15,'Blue',0,10,0)+
	Thorus(2,10,'Green',0,17,0)+
	Thorus(1,7,'Yellow',0,20,0)
```

When drawing 3D surfaces, you can choose to draw a wireframe, use polygon facets or use proper Phong Shading to emulate a curved surface.
In the latter example, surface normals are interpolated across all facets, creating a smooth appearance when it interact with light sources.

Example:

	theta:=Columns((0..360|5)°);
	phi:=Rows((0..360|5)°);
	R:=10;
	x:=R*cos(theta).*cos(phi);
	y:=R*sin(theta);
	z:=R*cos(theta).*sin(phi);
	samescale(linemesh3d(x+10,y+10,z+10)+polygonmesh3d(x+30,y+10,z+10)+surface3d(x+20,y+30,z+10))

```async
theta:=Columns((0..360|5)°);
phi:=Rows((0..360|5)°);
R:=10;
x:=R*cos(theta).*cos(phi);
y:=R*sin(theta);
z:=R*cos(theta).*sin(phi);
samescale(linemesh3d(x+10,y+10,z+10)+polygonmesh3d(x+30,y+10,z+10)+surface3d(x+20,y+30,z+10))
```

There are also specific 3D charts available that you can use. They may still require you to
prepare the input accordingly.

Example:

	[LabelsX,LabelsZ,Y]:=Histogram2D([Normal(0,1,100000),Normal(0,1,100000)],-5,5,50,-5,5,50);
	VerticalBars3D(Columns(LabelsX),Y,Rows(LabelsZ))

```async
[LabelsX,LabelsZ,Y]:=Histogram2D([Normal(0,1,100000),Normal(0,1,100000)],-5,5,50,-5,5,50);
VerticalBars3D(Columns(LabelsX),Y,Rows(LabelsZ))
```

#### Palette generation functions (Waher.Script.Fractals)

The following functions can be used to randomly create color palettes. The functions are available in the `Waher.Script.Fractals` library.
In an interactive script environment, clicking on the resulting graphs will zoom into the fractal, unless otherwise stated.

| Function           | Description | Example |
|--------------------|-------------|---------|
| `LinearColors(Colors[,N[,BandSize]])` | Creates a cyclic palette of `N` colors (default=1024) from an array of `Colors`, by linear interpolation over bands of `BandSize` intermediate colors (default=16). | `TestColorModel(LinearColors(["Red","Green","Blue"],1024,64))` |
| `RandomLinearAnalogousHSL([N[,BandSize[,Seed]]])` | Creates a palette of `N` colors (default=1024) consisting of bands of `BandSize` intermediate colors (default=16) interpolating random colors analogous in HSL space. The random number generator can be initialized by a `Seed`, if provided, or use a random one. | `TestColorModel(RandomLinearAnalogousHSL(1024,64))` |
| `RandomLinearAnalogousHSV([N[,BandSize[,Seed]]])` | Creates a palette of `N` colors (default=1024) consisting of bands of `BandSize` intermediate colors (default=16) interpolating random colors analogous in HSV space. The random number generator can be initialized by a `Seed`, if provided, or use a random one. | `TestColorModel(RandomLinearAnalogousHSV(1024,64))` |
| `RandomLinearComplementaryHSL([N[,BandSize[,Seed]]])` | Creates a palette of `N` colors (default=1024) consisting of bands of `BandSize` intermediate colors (default=16) interpolating random colors complementary in HSL space. The random number generator can be initialized by a `Seed`, if provided, or use a random one. | `TestColorModel(RandomLinearComplementaryHSL(1024,64))` |
| `RandomLinearComplementaryHSV([N[,BandSize[,Seed]]])` | Creates a palette of `N` colors (default=1024) consisting of bands of `BandSize` intermediate colors (default=16) interpolating random colors complementary in HSV space. The random number generator can be initialized by a `Seed`, if provided, or use a random one. | `TestColorModel(RandomLinearComplementaryHSV(1024,64))` |
| `RandomLinearRGB([N[,BandSize[,Seed]]])` | Creates a palette of `N` colors (default=1024) consisting of bands of `BandSize` intermediate colors (default=16) interpolating random colors in RGB space. The random number generator can be initialized by a `Seed`, if provided, or use a random one. | `TestColorModel(RandomLinearRGB(1024,64))` |
| `RandomSingleHue([N[,BandSize[,Seed]]])` | Creates a palette of `N` colors (default=1024) consisting of bands of `BandSize` intermediate colors (default=16) interpolating random colors using a single Hue. The random number generator can be initialized by a `Seed`, if provided, or use a random one. | `TestColorModel(RandomSingleHue(1024,64))` |
| `TestColorModel(Colors)` | Creates an image of color stripes defined by the vector of colors presented in `Colors`. | `TestColorModel(Palette)` |

#### Complex Fractal functions (Waher.Script.Fractals)

The following functions can be used to create fractal images based on iterations in the complex plane. The functions are available in the 
`Waher.Script.Fractals` library. They can be used as a means to create backgound images for themes, etc.

| Function                                | Description                                           | Example                                   |
|-----------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `HalleyBuilderFractal(z,dr,R[,Coefficients[,Palette[,DimX[,DimY]]]])` | Calculates a Halley Fractal Image. When clicked (in a GUI that supports user interaction with resulting images), adds a root to the underying polynomial, instead of zooming in. | `HalleyBuilderFractal((0,0),3,)` |
| `HalleyFractal(z,dr,R[,Coefficients[,Palette[,DimX[,DimY]]]])`<br/>`HalleyFractal(z,dr,R[,Lambda[,Palette[,DimX[,DimY]]]])` | Calculates a Halley Fractal Image. | `HalleyFractal((0,0),3,,[-1,0,0,0,0,0,1])` |
| `HalleySmoothFractal(z,dr,R[,Coefficients[,Palette[,DimX[,DimY]]]])`<br/>`HalleySmoothFractal(z,dr,R[,Lambda[,Palette[,DimX[,DimY]]]])` | As `HalleyFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `HalleySmoothFractal((0,0),3,,[-1,0,0,0,0,0,1])` |
| `HalleyTopographyFractal(z,dr,R[,Coefficients[,Palette[,DimX[,DimY]]]])`<br/>`HalleyTopographyFractal(z,dr,R[,Lambda[,Palette[,DimX[,DimY]]]])` | As `HalleyFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `HalleyTopographyFractal((0,0),3,,Uniform(0,5,8),,640,480)` |
| `JuliaFractal(z,c,dr[,Palette[,DimX[,DimY]]])`<br/>`JuliaFractal(z,Lambda,dr[,Palette[,DimX[,DimY]]])` | Calculates a Julia Fractal Image. | `JuliaFractal((0,0),(-0.785028076171875,-0.1465322265625),3,RandomLinearAnalogousHSL(1024,16,2056656298),640,480)`<br/>`JuliaFractal((0,0),z->(1,0.2)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))` |
| `JuliaInternalFractal(z,c,dr[,Palette[,DimX[,DimY]]])`<br/>`JuliaInternalFractal(z,Lambda,dr[,Palette[,DimX[,DimY]]])` | Calculates a Julia Fractal Image, and draws the internal of the fractal. | `JuliaInternalFractal((0,0),(-0.65,-0.4125),3,RandomLinearAnalogousHSL(16,4),800,600)` |
| `JuliaSmoothFractal(z,c,dr[,Palette[,DimX[,DimY]]])`<br/>`JuliaSmoothFractal(z,Lambda,dr[,Palette[,DimX[,DimY]]])` | As `JuliaFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `JuliaSmoothFractal((0,0),(-0.785028076171875,-0.1465322265625),3,RandomLinearAnalogousHSL(1024,16,2056656298),640,480)`<br/>`JuliaSmoothFractal((0,0),z->(1,0.2)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))` |
| `JuliaTopographyFractal(z,c,dr[,Palette[,DimX[,DimY]]])`<br/>`JuliaTopographyFractal(z,Lambda,dr[,Palette[,DimX[,DimY]]])` | As `JuliaFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `JuliaTopographyFractal((0,0),(-0.785028076171875,-0.1465322265625),3,RandomLinearAnalogousHSL(1024,16,2056656298),640,480)`<br/>`JuliaTopographyFractal((0,0),z->(1,0.2)*sin(z),7,RandomLinearAnalogousHsl(1024,16,21))` |
| `MandelbrotFractal(z,f,dr[,Palette[,DimX[,DimY]]])` | Calculates a Mandelbrot Fractal Image. | `MandelbrotFractal((-0.728474426269531,-0.240391845703126),,0.000732421875,RandomLinearRGB(4096,128,1325528060),640,480)`<br/>`MandelbrotFractal((1.13804443359375,-0.586863875325517),(z,c)->c*(z-z^2),6.103515625E-05,RandomLinearAnalogousHsl(1024,16,21),400,400)` |
| `MandelbrotSmoothFractal(z,f,dr[,Palette[,DimX[,DimY]]])` | As `MandelbrotFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `MandelbrotSmoothFractal((-0.728474426269531,-0.240391845703126),,0.000732421875,RandomLinearRGB(4096,128,1325528060),640,480)`<br/>`MandelbrotSmoothFractal((1.13804443359375,-0.586863875325517),(z,c)->c*(z-z^2),6.103515625E-05,RandomLinearAnalogousHsl(1024,16,21),400,400)` |
| `MandelbrotTopographyFractal(z,f,dr[,Palette[,DimX[,DimY]]])` | As `MandelbrotFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `MandelbrotTopographyFractal((-0.728474426269531,-0.240391845703126),,0.000732421875,RandomLinearRGB(4096,128,1325528060),640,480)`<br/>`MandelbrotTopographyFractal((1.13804443359375,-0.586863875325517),(z,c)->c*(z-z^2),6.103515625E-05,RandomLinearAnalogousHsl(1024,16,21),400,400)` |
| `NewtonBasinFractal(z,dr,R,c[,N[,DimX[,DimY]]])` | Creates a Newton basin fractal, coloring attractors found while executing the (generalized) Newton root finding algorithm in the complex plane. | `NewtonBasinFractal((0,0),3,,[-1,0,0,1])` |
| `NewtonBuilderFractal(z,dr,R[,Coefficients[,Palette[,DimX[,DimY]]]])` | Calculates a Newton Fractal Image. When clicked (in a GUI that supports user interaction with resulting images), adds a root to the underying polynomial, instead of zooming in. | `NewtonBuilderFractal((0,0),3,)` |
| `NewtonFractal(z,dr,R,c[,Palette[,DimX[,DimY]]])` | Calculates a Newton fractal. | `NewtonFractal((0,0),3,,[-1,0,0,0,0,1])`<br/>`NewtonFractal((pi/2,0),pi/2,2,x->tan(x),RandomLinearRGB(128,4,666001743),800,600)` |
| `NewtonSmoothFractal(z,dr,R,c[,Palette[,DimX[,DimY]]])` | As `NewtonFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `NewtonSmoothFractal((0,0),3,,[-1, 0, 0, 0, 0, 1],RandomLinearAnalogousHSL(128,4,746040511),640,480)` |
| `NewtonTopographyFractal(z,dr,R,c[,Palette[,DimX[,DimY]]])` | As `NewtonFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `NewtonTopographyFractal((0,0),3,,[-1, 0, 0, 0, 0, 1],RandomLinearAnalogousHSL(128,4,746040511),640,480)` |
| `NovaFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | Calculates a Nova fractal. | `NovaFractal(0,0,3,1.5,3,,640,480)` |
| `NovaSmoothFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `NovaSmoothFractal(0,0,3,1.5,3,,640,480)` |
| `NovaTopographyFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `NovaTopographyFractal(0,0,3,1.5,3,,640,480)` |
| `NovaJuliaFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | Calculates a Nova-Julia fractal. | `NovaJuliaFractal(0,0,0.1,0,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaJuliaSmoothFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaJuliaFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `NovaJuliaSmoothFractal(0,0,0.1,0,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaJuliaTopographyFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaJuliaFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `NovaJuliaTopographyFractal(0,0,0.1,0,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaMandelbrotFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | Calculates a Nova-Mandelbrot fractal. | `NovaMandelbrotFractal(0,0.1,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaMandelbrotSmoothFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaMandelbrotFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `NovaMandelbrotSmoothFractal(0,0.1,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaMandelbrotTopographyFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaMandelbrotFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `NovaMandelbrotTopographyFractal(0,0.1,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `SmoothImage(Image)` | Creates a smooth version of an image provided in `Image`. | `x:=-10..10;SmoothImage(plot2dcurve(x,sin(x),"Red",10))` |

#### Iterated Function System (IFS) Fractal functions (Waher.Script.Fractals)

The following functions can be used to create fractal images based on Iterated Function Systems (IFS). The functions are available in the 
`Waher.Script.Fractals` library. They can be used as a means to create backgound images for themes, etc.

| Function                                                                                                        | Description                                           | Example                                   |
|-----------------------------------------------------------------------------------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `FlameFractalHsl(xc,yc,dr,N,f[,Preview[,Parallel[,DimX[,DimY[,SuperSampling[,Gamma[,LightFactor[,Seed]]]]]]]])` | Calculates a flame fractal in HSL space. Intensity is mapped along the L-axis. Gamma correction is done along the SL-axes. The L-axis is multiplicated with the LightFactor. | `FlameFractalHsl(0.6109375,0.199208333333333,0.625,1e7,[Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",Identity(2),DiamondVariation(),"Red"],False,False,400,300,1,2.5,2,1668206157)` |
| `FlameFractalRgba(xc,yc,dr,N,f[,Preview[,Parallel[,DimX[,DimY[,SuperSampling[,Gamma[,Vibrancy[,Seed]]]]]]]])`   | Calculates a flame fractal in RGBA space. Intensity is calculated along the A-axis. Gamma correction is done along the RGB-axes (vibrancy=0) or along the A-axis (vibrancy=1), or a combination thereof. | `FlameFractalRgba(0,0,0,1e7,[Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",ExponentialVariation(),Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",ExponentialVariation()],400,300)` |
| `FlameFractal(xc,yc,dr,N,f[,Preview[,Parallel[,DimX[,DimY[,SuperSampling[,Gamma[,Vibrancy[,Seed]]]]]]]])`       | Alias for `FlameFractalRgba`. | `FlameFractal(0,0,0,1e7,[Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",ExponentialVariation(),Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",ExponentialVariation()],400,300)` |
| `EstimateFlameSize(N,f[,DimX[,DimY[,Seed]]])`                                                                   | Estimates the dimensions necessary for a flame fractal. Can be used to check if a selected set of parameters diverges or not. A vector is returned with x- and y-coordinates, as well as size `[xc, yc, dr]`. | `EstimateFlameSize(1e7,[Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",Identity(2),DiamondVariation(),"Red"],400,300)` |
| `IfsFractal(xc,yc,dr,N,T[,DimX[,DimY[,Seed]]])`                                                                 | Calculates a fractal based on an Iterated Function System, using the chaos game. | ` IfsFractal(0,5,6,2e6,[[[0,0,0],[0,0.16,0],[0,0,1]],0.01,"Green",[[0.85,0.04,0],[-0.04,0.85,1.6],[0,0,1]],0.85,"Green",[[0.2,-0.26,0],[0.26,0.24,1.6],[0,0,1]],0.07,"Green",[[-0.15,0.28,0],[0.26,0.24,0.44],[0,0,1]],0.07,"Green"],300,600);` |

`IfsFractals` run on Iterated Function Systems using systems of equations, with optional color coding and weights. These equations, color codings
and weights are provided in the vector `T`. These equations can be given by matrices representing linear transforms, or lambdra expressions. 
Examples:

	IfsFractal(0.5,0.5,1,1e6, [
		Scale2DH(0.5,0.5),
		Translate2DH(0.25,0.5)*Scale2DH(0.5,0.5),
		Translate2DH(0.5,0)*Scale2DH(0.5,0.5)],400,400);

	IfsFractal(0,0,3,1e6, [
		 z->sqrt(z-(-0.748814392089844-0.0801434326171877*i)),
		 z->-sqrt(z-(-0.748814392089844-0.0801434326171877*i))],400,400);

	IfsFractal(0,5,6,1e6, [
		 [[0,0,0],
		 [0,0.16,0],
		 [0,0,1]],
		 0.01,"Green",
		 [[0.85,0.04,0],
		 [-0.04,0.85,1.6],
		 [0,0,1]],
		 0.85,"Green",
		 [[0.2,-0.26,0],
		 [0.26,0.24,1.6],
		 [0,0,1]],
		 0.07,"Green",
		 [[-0.15,0.28,0],
		 [0.26,0.24,0.44],
		 [0,0,1]],
		 0.07,"Green"],300,600);

Flame fractals can modify the linear transforms using one or more *variations*. (Variations can be seen as non-linear "distortions", or modifiers, 
to the iterated function system, to create more interesting visual results.) There are also two rendering modes: HSL and RGBA.

Example:

	FlameFractalRgba(0,0,0,1e7,[
		Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",ExponentialVariation(),
		Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",ExponentialVariation()
		],400,300)

There are many different types of predefined variations[^For more information about Flame Fractals and variations, see the paper on 
<a href="http://flam3.com/flame.pdf" target="_blank">The Fractal Flame Algorithm</a> by Scott Daves and Erik Reckase] that can be used.
You can also use the `LambdaVariation` to create your custom variations.

| Flame variations        | Complex variations         | Fractal variations    |
|-------------------------|----------------------------|-----------------------|
| `XVariation`			  |  `zCosVariation`		   | `JuliaRoot1Variation` |
| `ArchVariation`		  |	 `zCubeVariation`		   | `JuliaRoot2Variation` |
| `Bent2Variation`		  |	 `zDivVariation`		   | `JuliaStepVariation`  |
| `BentVariation`		  |	 `zExpVariation`		   |                       |
| `BladeVariation`		  |	 `zLnVariation`			   |					   |
| `BlobVariation`		  |	 `zLogNVariation`		   |					   |
| `BlurVariation`		  |	 `zMulVariation`		   |					   |
| `BubbleVariation`		  |	 `zPowerBaseVariation`	   |					   |
| `ConicVariation`		  |	 `zPowerExponentVariation` |					   |
| `CosineVariation`		  |	 `zSinHVariation`          |					   |
| `CrossVariation`		  |	 `zSinVariation`		   |					   |
| `CurlVariation`		  |	 `zSqrtVariation`		   |					   |
| `CylinderVariation`	  |	 `zSqrVariation`		   |					   |
| `DiamondVariation`	  |	 `zTanHVariation`		   |					   |
| `Disc2Variation`		  |	 `zTanVariation`		   |					   |
| `DiscVariation`		  |	 `zACosVariation`		   |					   |
| `ExponentialVariation`  |	 `zASinVariation`		   |					   |
| `EyeFishVariation`	  |	 `zATanVariation`		   |					   |
| `Fan2Variation`		  |	 `zConjugateVariation`	   |					   |
| `FanVariation`		  |	 `zCosHVariation`		   |					   |
| `FishEyeVariation`	  |                            |					   |
| `FlowerVariation`		  |							   |					   |
| `GaussianVariation`	  |							   |					   |
| `HandkerchiefVariation` |							   |					   |
| `HeartVariation`        |							   |					   |
| `HorseShoeVariation`	  |							   |					   |
| `HyperbolicVariation`	  |							   |					   |
| `JuliaNVariation`		  |							   |					   |
| `JuliaScopeVariation`	  |							   |					   |
| `JuliaVariation`		  |							   |					   |
| `LinearVariation`		  |							   |					   |
| `NGonVariation`		  |							   |					   |
| `NoiseVariation`		  |							   |					   |
| `QuadraticVariation`	  |							   |					   |
| `ParabolaVariation`	  |							   |					   |
| `PdjVariation`		  |							   |					   |
| `PerspectiveVariation`  |							   |					   |
| `PieVariation`		  |							   |					   |
| `PolarVariation`		  |							   |					   |
| `PopcornVariation`	  |							   |					   |
| `PowerVariation`		  |							   |					   |
| `RadialBlurVariation`	  |							   |					   |
| `RaysVariation`		  |							   |					   |
| `RectanglesVariation`	  |							   |					   |
| `Rings2Variation`		  |							   |					   |
| `RingsVariation`		  |							   |					   |
| `Secant2Variation`	  |							   |					   |
| `SecantVariation`		  |							   |					   |
| `SinusoidalVariation`	  |							   |					   |
| `SphericalVariation`	  |							   |					   |
| `SpiralVariation`		  |							   |					   |
| `SquareVariation`		  |							   |					   |
| `SuperShapeVariation`	  |							   |					   |
| `SwirlVariation`		  |							   |					   |
| `TangentVariation`	  |							   |					   |
| `TwintrianVariation`	  |							   |					   |
| `WavesVariation`		  |							   |					   |

#### External Database-related functions (Waher.Script.Data)

| Function | Description | Example |
|----------|-------------|---------|
| `Callback(DelegateType,Lambda)`                      | Creates a callback function based on a script-based lambda-expression. The lambda expression must have the same number of arguments as defined by the delegate type. | `Callback(DelegateType,Lambda)` |
| `Callback(DelegateType[,TArg1],Lambda)`              | Creates a callback function based on a script-based lambda-expression. The lambda expression must have the same number of arguments as defined by the generic delegate type and argument type. | `Callback(DelegateType,TArg1,Lambda)` |
| `ConnectMsSql(ConnectionString[,UserName,Password])` | Connects to an external Microsoft SQL Server database using a connection string, and optionally providing credentials. | `db:=ConnectMsSql(cs,UserName,Password)` |
| `ConnectMsSql(Host,Database[,UserName,Password])`    | Connects to an external Microsoft SQL Server database `Database` hosted by a machine reachable on `Host`, providing optional credentials. If no credentials are provided, Integrated Security is used. | `db:=ConnectMsSql("Host","Database",UserName,Password)` |
| `ConnectOdbc(ConnectionString[,UserName,Password])`  | Connects to an external ODBC database using a connection string, and optionally providing credentials. | `db:=ConnectOdbc(cs,UserName,Password)` |
| `ConnectOleDb(ConnectionString[,UserName,Password])` | Connects to an external OLE DB database using a connection string, and optionally providing credentials. | `db:=ConnectOleDb(cs,UserName,Password)` |

**Note**: To close a connection, destroy the variable containing the connection using the Destroy or Delete function.

##### Closing a connection

To close a connection, destroy the variable containing the connection using the Destroy or Delete function:

```
Destroy(db)
```

##### Executing custom SQL on an extenral database

To execute custom SQL on an external database, treat the connection as a lambda expression taking one parameter: The script to execute.
Any results or result sets will be returned to the script after completing the execution. Example:

```
Nr:=db("SELECT COUNT(*) from city");
```

##### Stored Procedures

Stored Procedures in the database are available directly by reference on the connection object. If you have a stored procedure in the
database named `proc` that takes two arguments `P1` and `P2`, and a connection that you have stored in `db`, you call the procedure
as follows:

```
db.proc(P1,P2)
```

You can also create a lambda expression to the stored procedure, and store it in a variable for reference:

```
f:=db.proc;
f(P1,P2)
```

This syntax is available for all database conncetions that support stored procedures.

#### External MySQL Database-related functions (Waher.Script.Data.MySQL)

This module adds external database support for MySQL databases. Connections established to MySQL databases work in script, as
all connection types defined in the [Waher.Script.Data](#externalDatabaseRelatedFunctionsWaherScriptData) library, so the corresponding
subsections will not be repeated here.

| Function | Description | Example |
|----------|-------------|---------|
| `ConnectMySql(ConnectionString[,UserName,Password])` | Connects to an external MySQL Server database usig a connection string, and optionally providing credentials. | `db:=ConnectMySql(cs,UserName,Password)` |
| `ConnectMySql(Host,Database,UserName,Password)`      | Connects to an external MySQL Server database `Database` hosted by a machine reachable on `Host`, and providing credentials. | `db:=ConnectMySql("Server","Database",UserName,Password)` |

#### External PostgreSQL Database-related functions (Waher.Script.Data.PostgreSQL)

This module adds external database support for PostgreSQL databases. Connections established to PostgreSQL databases work in script, as
all connection types defined in the [Waher.Script.Data](#externalDatabaseRelatedFunctionsWaherScriptData) library, so the corresponding
subsections will not be repeated here.

| Function | Description | Example |
|----------|-------------|---------|
| `ConnectPostgreSql(ConnectionString[,UserName,Password])` | Connects to an external PostgreSQL Server database using a connection string, and optionally providing credentials. | `db:=ConnectPostgreSql(cs,UserName,Password)` |
| `ConnectPostgreSql(Host,Database,UserName,Password)`      | Connects to an external PostgreSQL Server database `Database` hosted by a machine reachable on `Host`, and providing credentials. | `db:=ConnectPostgreSql("Server","Database",UserName,Password)` |

#### Full-text-search functions (Waher.Script.FullTextSearch)

This module adds full-text-search capabilities to script, using the full-text-search
module provided in `Waher.Persistence.FullTextSearch` library. The following functions
are available:

| Function | Description | Example |
|----------|-------------|---------|
| `AddFtsProperties(Collection,Properties)`                                        | Adds properties defined by the vector `Properties` to the list of properties in the corresponding full-text-search index to the collection named in the collection defined by `Collection`. The function returns a Boolean value indicating if the call represents a change in the configuration. | `AddFtsProperties("Default",["Prop1","Prop2"])` |
| `FtsCollection(Index,Collection)`                                                | Matches a Full-Text-Search Index, defined by `Index`, with a Database Collection, defined by `Collection`. | `FtsCollection("FTS","Default")` |
| `FtsFile(Index,FileName)`                                                        | Indexes (or reindexes) a specific file, using the full-text-search collection index defined by `Index`. If the file does not exist, it is removed from the index. | `FtsFolder("FTS",Folder,true)` |
| `FtsFolder(Index,Folder[,Recursive[,SubFoldersToExclude]])`                      | Indexes files in a folder given by `Folder`, using the full-text-search collection index defined by `Index`. Files can be processed recursively in subfolders if `Recursive` is `true` (default is `false`). You can exclude subfolders by providing such a folder or a vector of such folders in `SubFoldersToExclude`. To keep folder updated, call `FtsFile` when a file is modified, created or deleted. `FtsFolder` only updates files who have not been indexed before, or whose timestamps have changed since last indexation. | `FtsFolder("FTS",Folder,true)` |
| `GetFtsCollections([Index])`                                                     | Gets database collections indexed for full-text-search. If an index name is provided, a vector of collection names related to that index will be returned. If no arguments are provided, a dictionary will be returned, with collection vectors for each corresponding index will be returned. | `GetFtsCollections("FTS")` |
| `GetFtsProperties([Collection])`                                                 | Gets properties indexed for full-text-search. If a collection name is provided, a vector of property names related to that collection will be returned. If no arguments are provided, a dictionary will be returned, with property vectors for each corresponding collection will be returned. | `GetFtsProperties("Default")` |
| `RemoveFtsProperties(Collection,Properties)`                                     | Removes properties defined by the vector `Properties` from the list of properties in the corresponding full-text-search index to the collection named in the collection defined by `Collection`. The function returns a Boolean value indicating if the call represents a change in the configuration. | `RemoveFtsProperties("Default",["Prop1","Prop2"])` |
| `ReindexFts(Index)`                                                              | Reindexes the full-text-search index defined by `Index`. This process may take some time, as all objects in the corresponding collections will be iterated and reindexed. | `ReindexFts("FTS")` |
| `Search(Index,Query,Strict[,Offset,MaxCount[,Order[,Type,PaginationStrategy]]])` | Performs a full-text-search of the query `Query` in the full-text-search index `Index`. `Strict` controls if keywords are as-is (`true`) or prefixes (`false`). Pagination is controlled by `Offset` abd `MaxCount`. The sort order is defined by `Order`. Typed searches can be performed, controlled by the optional arguments `Type` and `PaginationStrategy`. | `Search("FTS","Kilroy was here",0,25,"Relevance")` |

##### Full-text-search query syntax

Full-text-search is done providing a query string. This query string contains *keywords*
separated by whitespace. Keywords only consist of *letters* and *digits*, and are
*case insensitive*. Punctuation characters, accents, etc., are ignored. So are a 
configurable set of *stop words*, common words that have little significance in
full-text-search. If the keyword is prefixed with a `+`, it is required to exist. 
If it is prefixed by `-`, it is prohibited. No prefix means it is optional. Wildcards 
are permitted in keywords. You can use any of the characters `*`, `%`, `¤` or `#` as 
wildcard characters. You can also use regular expressions by encapsulating it between 
`/` characters, such as `/regex/`. You can search for sequences of keywords by 
encapsulating them between apostrophes `'` or quotes `"`.

| Summary                                                                                    |||
| Syntax             | Example                           | Discription                         |
|:-------------------|:----------------------------------|:------------------------------------|
| `keyword`          | `kilroy`                          | Letters or digits, case-insensitive |
| `+`                | `+kilroy`                         | Required keyword                    |
| `-`                | `-kilroy`                         | Prohibited keyword                  |
| `*`, `%`, `¤`, `#` | `kil*`, `%roy`, `k¤r#`            | Wildcards                           |
| `/regex/`          | <code>/(kil&#124;fitz)roy/</code> | Regular expression                  |
| `'`, `"`           | `"kilroy was here"`               | Sequence of keywords                |

The syntax can be nested, so you can combine the different constructs.

| Composite Examples                                |
|:--------------------------------------------------|
| `Kilroy was here.`                                |
| `+Kilroy was here.`                               |
| `+Kilroy was -not here.`                          |
| `+*roy was -not here.`                            |
| `+Kil* was -not here.`                            |
| `+K*y was -not here.`                             |
| <code>+/(Kil&#124;Fitz)roy/ was -not here.</code> |
| <code>+/Kil(roy&#124;ling)/ was -not here.</code> |
| `+/K.+y/ was -not here.`                          |
| `+'Kilroy was here'`                              |
| `'Kilroy was' here`                               |
| `Kilroy 'was here'`                               |

**Note**: The words `was` and `here` are used as examples only, to highlight syntax. 
They are typically considered *stop words*, and thus ignored in a real search.

##### Strictness

Strictness in search is controlled by the `Strict` argument in the `Search` function.
If `true`, each keyword is interpreted as-is. The `keyword` will match `search`, but
not `searching`. If `Strict` is `false`, keywords are interpreted as *prefixes*.
The keyword `search` will therefore also match `searching`.

##### Search order

Search order in the `Search` function is controlled by the optional `Order` arguments.
If not provided, `Relevance` is used. You can provide string values, or an enumeration
value of type `Waher.Persistence.FullTextSearch.FullTextSearchOrder`. Possible values:

| Order         | Description |
|:--------------|:------------|
| `Relevance`   | Objects are ordered, first by number of distinct keywords found in each object, then by total number of occurrences of keywords, then indexation timestmap. |
| `Occurrences` | Objects are ordered, first by number of occurrences of keywords found in each object, then by number of distinct keywords, then indexation timestmap. |
| `Newest`      | Objects are ordered, by indexation timestamp, newest objects first. |
| `Oldest`      | Objects are ordered, by indexation timestamp, oldest objects first. |

##### Pagination Strategy

When searching for typed objects, especially in full-text-search collections indexing
objects of multiple types, a strategy for how to handle pagination and incompatible types
is necessary, for performance reasons. This is controlled by the optional `PaginationStrategy`
argument in the `Search` function, together with the `Type` argument, that controls the
type of objects to search for. You can provide a string value, or an enumeration value
of type `Waher.Persistence.FullTextSearch.PaginationStrategy`. The default value is
`PaginateOverObjectsNullIfIncompatible`, which is the fastest option. But incompatible
objects will be returned as `null`. The following values are available.

| Strategy      | Description |
|:--------------|:------------|
| `PaginateOverObjectsNullIfIncompatible` | Pagination is done over objects found in search. Incompatible types are returned as null. Makes pagination quicker, as objects do not need to be preloaded, and can be skipped quicker. |
| `PaginateOverObjectsOnlyCompatible`     | Pagination is done over objects found in search. Only compatible objects are returned. Amount of objects returned might be less than number of objects found, making evaluation of next offset in paginated search difficult. |
| `PaginationOverCompatibleOnly`          | Pagination is done over compatible objects found in search. Pagination becomes more resource intensive, as all objects need to be loaded to be checked if they are compatible or not. |

#### Networking-related functions (Waher.Script.Networking)

The following functions are available in the `Waher.Script.Networking` library.

| Function | Description | Example |
|----------|-------------|---------|
| `Dns(Name[,QTYPE[,QCLASS]])` | Makes a DNS request to resolve a name, given a QTYPE (default `QTYPE.A`) and QCLASS (default `QCLASS.IN`). | `DNS(Domain,QTYPE.TXT)` |
| `Ping(Host)`                 | Performs an ICMP Echo to the `Host`, and returns the roundtrip time. | `ping('example.com')` |
| `Rdap(Ip)`                   | Makes an RDAP request to provide JSON information about an IP-address.    | `RDAP("1.2.3.4")`  |
| `RDns(IpAddress)`            | Makes a Reverse DNS lookup of an IP Address. | `RDNS("1.2.3.4")` |
| `Route(Host)`                | Uses the ICMP Echo protocol to find the route in the IP network to a given host. | `route('example.com')` |
| `WhoIs(Ip)`                  | Makes a WHOIS request to provide textual information about an IP-address. | `WHOIS("1.2.3.4")` |

#### Persistence-related functions (Waher.Script.Persistence)

The following functions are available in the `Waher.Script.Persistence` library.

| Function                                                 | Description | Example |
|----------------------------------------------------------|-------------|---------|
| `DecCounter(CounterName[,Amount])`                       | Decrements a counter, given its name, and returns the decremented count. | `DecCounter("NrTests")` |
| `DeleteObject(Obj)`                                      | Deletes an object from the underlying persistence layer. | `DeleteObject(Obj)` |
| `FindObjects(Type, Offset, MaxCount, Filter, SortOrder)` | Finds objects of a given `Type`. `Offset` and `MaxCount` provide a means to paginate the result set. `Filter` can be null, if none is used, or a string containing an expression to limit the result set. `SortOrder` sorts the result. It also determines the index to use. | `FindObjects(Namespace.CustomType, 0, 10, "StringProperty='StringValue'", ["Property1","Property2"])` |
| `Generalize(Object)`                                     | Creates a generalized representation of the data in the object `Object`. This generalized representation can be more easily serialized, to JSON for instance, or to an object database. | `generalize(Obj)` |
| `GetCounter(CounterName)`                                | Gets the current count of a counter, given its name. | `GetCounter("NrTests")` |
| `GetSetting([Host,]Name,DefaultValue)`                   | Gets a runtime setting with name `Name`. If `Host` is provided, the function first tries to get the corresponding runtime host setting, and if one is not found, the corresponding runtime setting. If one is not found, the `DefaultValue` value is returned. `Host` can be a string, or an object with a host reference (implementing the `Waher.Content.IHostReference` interface), such as a HTTP Request object or similar. | `GetSetting("Name","Kilroy")` |
| `GetHostSetting(Host,Name,DefaultValue)`                 | Gets a runtime host setting with name `Name`. `Host` defines the host. If a host runtime setting is not found, the `DefaultValue` value is returned. `Host` can be a string, or an object with a host reference (implementing the `Waher.Content.IHostReference` interface), such as a HTTP Request object or similar. | `GetHostSetting(Request,"Name","Kilroy")` |
| `GetUserSetting(User,Name,DefaultValue)`                 | Gets a runtime user setting with name `Name`. `User` defines the user. If a user runtime setting is not found, the `DefaultValue` value is returned. `User` can be a string, or an object with a user reference (implementing the `Waher.Security.IUser` interface), or an origin reference (implementing the `Waher.Things.IRequestOrigin` interface). | `GetUserSetting(User,"Name","Kilroy")` |
| `IncCounter(CounterName[,Amount])`                       | Increments a counter, given its name, and returns the incremented count. | `IncCounter("NrTests")` |
| `PersistHash([Realm,]Hash)`                              | Persists a Hash digest in an optional realm. The function returns `true` if the hash was persisted and `false` if it was already persisted. | `PersistHash("Secrets",Sha2_256(Secret))` |
| `Pivot(Result)`                                          | Pivots a result matrix so columns become rows, and vice versa. It is similar to the matrix transpose operator, except it takes column headers into account also. | `pivot(select Type, count(*) Nr from PersistedEvent group by Type)` |
| `SaveNewObject(Obj)`                                     | Saves a new object to the underlying persistence layer. | `SaveNewObject(Obj)` |
| `SetSetting([Host,]Name,Value)`                          | Sets a runtime setting with name `Name`. If `Host` is provided, the corresponding runtime host setting is set. `Host` can be a string, or an object with a host reference (implementing the `Waher.Content.IHostReference` interface), such as a HTTP Request object or similar. | `SetSetting("Name","Kilroy")` |
| `SetHostSetting(Host,Name,Value)`                        | Sets a runtime host setting with name `Name`. `Host` defines the host. `Host` can be a string, or an object with a host reference (implementing the `Waher.Content.IHostReference` interface), such as a HTTP Request object or similar. | `SetHostSetting(Request,"Name","Kilroy")` |
| `SetUserSetting(User,Name,Value)`                        | Sets a runtime user setting with name `Name`. `User` defines the user. `User` can be a string, or an object with a user reference (implementing the `Waher.Security.IUser` interface), or an origin reference (implementing the `Waher.Things.IRequestOrigin` interface). | `SetHostSetting(Request,"Name","Kilroy")` |
| `Specialize(Object)`                                     | Creates a specialized representation of the data in the object `Object`, which would be a generic object from the database, or similar. The specialized object would be represented using a proper object instance of the indicated type. | `specialize(Obj)` |
| `UpdateObject(Obj)`                                      | Updates an object in the underlying persistence layer. | `UpdateObject(Obj)` |
| `VerifyHash([Realm,]Hash)`                               | Verifies a Hash digest in an optional realm. The function returns `true` if the hash was found and `false` if not. | `VerifyHash("Secrets",Sha2_256(Secret))` |
| `XPath(Expression)`                                      | Specifies an XPath-expression. | `XPath("Element/@Attr")` |

In addition to this, the following XPATH/SPARQL extension functions are defined

| Function FQN                                | Description |
|:--------------------------------------------|:------------|
| `http://www.w3.org/2001/XMLSchema#boolean`  | Converts a value to a Boolean literal value. |
| `http://www.w3.org/2001/XMLSchema#dateTime` | Converts a value to a date and time value. |
| `http://www.w3.org/2001/XMLSchema#decimal`  | Converts a value to a decimal-precision floating point literal value. |
| `http://www.w3.org/2001/XMLSchema#double`   | Converts a value to a double-precision floating point literal value. |
| `http://www.w3.org/2001/XMLSchema#float`    | Converts a value to a single-precision floating point literal value. |
| `http://www.w3.org/2001/XMLSchema#integer`  | Converts a value to an integer literal value. |
| `http://www.w3.org/2001/XMLSchema#string`   | Converts a value to an string literal value. |

#### Statistics-related functions (Waher.Script.Statistics)

The following functions are available in the `Waher.Script.Statistics` library.

| Function | Description | Example |
|----------|-------------|---------|
| `Beta(Alpha,Beta[,N]])`                    | Generates a random number using the Beta distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Beta(2,5,10000),0,1,10);VerticalBars(Labels,Counts)` |
| `Cauchy(Median,Scale[,N]])`                | Generates a random number using the Cauchy distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Cauchy(5,1.5,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Chi2(Degrees[,N]])`                       | Generates a random number using the Chi squared distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Chi2(6,10000),0,20,10);VerticalBars(Labels,Counts)` |
| `erf(z)`                                   | Error function. | `erf(t2)-erf(t1)` |
| `Exponential([Mean[,N]])`                  | Generates a random number using the Exponential distribution. If no `Mean` is given, the mean is assumed to be 1. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Exponential(3,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Gamma(z)`                                 | Computes `Γ(z)` | `Gamma(2.5)` |
| `Gamma(Shape,Scale[,N]])`                  | Generates a random number using the Gamma distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Gamma(3,3,10000),0,20,10);VerticalBars(Labels,Counts)` |
| `Histogram(V,Min,Max,N)`                   | Calculates the histogram of a set of data `V` with `N` buckets between `Min` and `Max`. | `[Labels,Counts]:=Histogram(Uniform(0,10,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Histogram2D(M,MinX,MaxX,NX,MinY,MaxY,NY)` | Calculates the two-dimensional histogram of a set of data points `M` with `NX` buckets between `MinX` and `MaxX` for the first coordinate, and `NY` buckets between `MinY` and `MaxY` for the second coordinate. | `[LabelsX,LabelsY,Counts]:=Histogram2D([Normal(0,1,10000),Normal(0,1,10000)],-5,5,10,-5,5,10);VerticalBars3D(Columns(LabelsX),Counts,Rows(LabelsY))` |
| `Laplace(Mean,Scale[,N]])`                 | Generates a random number using the Laplace distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Laplace(5,1.5,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `LGamma(a,z)`                              | Computes the lower incomplete gamma function `γ(a,z)` | `LGamma(2.5,8)` |
| `Normal([Mean,StdDev][,N]])`               | Generates a random number using the Normal distribution. If no `Mean` and standard deviation `StdDev` is given, the mean is assumed to be 0 and standarddeviation assumed to be 1. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Normal(0,5,10000),-20,20,10);VerticalBars(Labels,Counts)` |
| `StudentT(Degrees[,N]])`                   | Generates a random number using the Student-T distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(StudentT(6,10000),-5,5,10);VerticalBars(Labels,Counts)` |
| `UGamma(a,z)`                              | Computes the upper incomplete gamma function `Γ(a,z)` | `UGamma(2.5,8)` |
| `Uniform([Min,Max][,N]])`                  | Generates a random number using the Uniform distribution. If no interval is given, the standard interval [0,1] is assumed. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Uniform(0,10,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Weibull(Shape,Scale[,N]])`                | Generates a random number using the Weibull distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Weibull(5,3,10000),0,10,10);VerticalBars(Labels,Counts)` |

#### System-related functions (Waher.Script.System)

The following functions are available in the `Waher.Script.Statistics` library.

| Function | Description |
|----------|-------------|
| `ShellExecute(FileName,Arguments,WorkFolder)` | Starts a process and executes a shell command-line instruction. |

#### Threading-related functions (Waher.Script.Threading)

The following functions are available in the `Waher.Script.Threading` library.

| Function                                | Description | Example |
|-----------------------------------------|-------------|---------|
| `Abort(BackgroundId)`                   | Aborts the background processing of script identified by `BackgroundId`. The function returns `true` if the task was found and aborted, `false` if the task was not found, or has completed. | Abort(Id) |
| `Background(Script)`                    | Executes the provided script asynchronously in the background. The function does not wait for the result to be completed. The script runs on a separate variable collection, that is initialized with the variables available to the function when executed. The function returns an ID that can be used to abort the process, by calling the `Abort` function. | Background(f(a,b)) |
| `Semaphore(Name,Script)`                | Protects the evaluation of the script defined by `Script` by using a semaphore of name `Name`. Only one thread can evaluate script in a named semaphore at the same time. | Semaphore('Lock1',f(a,b)) |
| `Sleep(ms)`                             | Sleeps for a certain number of milliseconds, without consuming processor power. | `Sleep(100)` |
| `Parallel(Tasks[,Tasks[,Tasks[,...]]])` | Evaluates tasks in parallel. These tasks can either be arguments to the function, or expressed as items in vector arguments. Elements in such vector arguments are evaluated in parallel. Each task receives its own cloned variables collection. | `Parallel([f(a,b), f(c,d)])` |

Example of parallel execution:

```
f(s):=
(
	Start:=Now;
	foreach x in 1..10 do
	(
		Sleep(Uniform(500,1500));
		Semaphore("Output",printline(s+": "+x));
	);
	Now.Subtract(Start).TotalSeconds
);

Parallel(f("A"),f("B"),f("C"))
```

#### Content-related functions (Waher.Script.Content)

The following functions are available in the `Waher.Script.Content` library.

| Function                                           | Description | Example |
|----------------------------------------------------|-------------|---------|
| `Base64Decode(Data)`                               | Decodes binary data from a string using BASE64 encoding. | [Example][Base64DecodeExample] |
| `Base64Encode(Data)`                               | Encodes binary data to a string using BASE64 encoding. | [Example][Base64EncodeExample] |
| `Base64UrlDecode(Data)`                            | Decodes binary data from a string using BASE64URL encoding. | [Example][Base64UrlDecodeExample] |
| `Base64UrlEncode(Data)`                            | Encodes binary data to a string using BASE64URL encoding. | [Example][Base64UrlEncodeExample] |
| `CustomEncode(Binary,ContentType)`                 | Can be used to return custom encoded data from web services. | [Example][CustomEncodeExample] |
| `Decode(Content,Type)`                             | Decodes `Content` using the available Internet Content Type decoder for Content Type `Type`. | [Example][DecodeExample] |
| `Delete(Url,Accept/Headers,[Certificate])`         | Deletes a resource, in accordance with the [URI scheme](#uriSchemes) of the `Url`, and decodes the response, in accordance with the content type returned. The second argument is required, to differ the function from the `Delete(x)` function, that destroys a variable `x` and disposes of its value. The headers can be an empty object `{}`. If providing a `Certificate`, mutual TLS can be used. | [Example][DeleteExample] |
| `Duration(s)`                                      | Parses a string `s` into a Duration value. | `Duration("PT10H30M")` |
| `Duration(Years,Month,Dats,Hours,Minutes,Seconds)` | Creates a Duration value. | `Duration(0,0,0,10,30,0)` |
| `Encode(Object[,Types])`                           | Encodes `Object` using the available Internet Content Type encoders. If `Types` is provided, it is an array of acceptable content types that can be used. The result is a two-dimensional vector, containing the binary encoding as the first element, and the content type as the second element. | [Example][EncodeExample] |
| `FileAttributes(FileName)`                         | Gets the attributes of a file, given its full file name. | [Example][FileAttributesExample] |
| `FileCreationTime(FileName)`                       | Gets the creation time of a file, given its full file name. | [Example][FileCreationTimeExample] |
| `FileCreationTimeUtc(FileName)`                    | Gets the creation time (in UTC) of a file, given its full file name. | [Example][FileCreationTimeUtcExample] |
| `FileLastAccessTime(FileName)`                     | Gets the last access time of a file, given its full file name. | [Example][FileLastAccessTimeExample] |
| `FileLastAccessTimeUtc(FileName)`                  | Gets the last access time (in UTC) of a file, given its full file name. | [Example][FileLastAccessTimeUtcExample] |
| `FileLastWriteTime(FileName)`						 | Gets the last write time of a file, given its full file name. | [Example][FileLastWriteTimeExample] |
| `FileLastWriteTimeUtc(FileName)`					 | Gets the last write time (in UTC) of a file, given its full file name. | [Example][FileLastWriteTimeUtcExample] |
| `Get(Url[,Accept/Headers[,Certificate]])`          | Retrieves a resource, in accordance with the [URI scheme](#uriSchemes) of the `Url`, and decodes it, in accordance with its content type. If a second argument is provided, it either represents an `Accept` header, if a string, or custom protocol-specific headers or options, if an object. If providing a `Certificate`, mutual TLS can be used. | [Example][GetExample] |
| `Head(Url[,Accept/Headers[,Certificate]])`         | Retrieves the headers of a resource, in accordance with the [URI scheme](#uriSchemes) of the `Url`. If a second argument is provided, it either represents an `Accept` header, if a string, or custom protocol-specific headers or options, if an object. If providing a `Certificate`, mutual TLS can be used. | [Example][HeadExample] |
| `HtmlAttributeEncode(s)`                           | Encodes a string for inclusion in an HTML attribute. It transforms `<`, `>`, `&` and `"` to `&lt;`, `&gt;`, `&amp;` and `&quot;` correspondingly. | [Example][HtmlAttributeEncodeExample] |
| `HtmlValueEncode(s)`                               | Encodes a string for inclusion as an HTML element value. It transforms `<`, `>` and `&` to `&lt;`, `&gt;` and `&amp;` correspondingly. | [Example][HtmlValueEncodeExample] |
| `JsonDecode(Data)`                                 | Decodes a JSON string. | [Example][JsonDecodeExample] |
| `JsonEncode(Data)`                                 | Encodes data into a JSON string. | [Example][JsonEncodeExample] |
| `LoadFile(FileName[,ContentType])`                 | Loads a file and decodes it. By default, the content type defined by the file extension is used, if defined. You can also explicitly provide a content type. | [Example][LoadFileExample] |
| `Post(Url,Data[,Accept/Headers,[Certificate]])`    | Encodes data and posts it to a resource, in accordance with the [URI scheme](#uriSchemes) of the `Url`, and decodes the response, in accordance with the content type returned. If a third argument is provided, it either represents an `Accept` header, if a string, or custom protocol-specific headers or options, if an object. If providing a `Certificate`, mutual TLS can be used. | [Example][PostExample] |
| `PrettyJson(Obj)`                                  | JSON-encodes `Obj` as a pretty string, for display. | [Example][PrettyJsonExample] |
| `Put(Url,Data[,Accept/Headers,[Certificate]])`     | Encodes data and PUTs it to a resource, in accordance with the [URI scheme](#uriSchemes) of the `Url`, and decodes the response, in accordance with the content type returned. If a third argument is provided, it either represents an `Accept` header, if a string, or custom protocol-specific headers or options, if an object. If providing a `Certificate`, mutual TLS can be used. | [Example][PutExample] |
| `QrEncode(Text,Level)`                             | Encodes text in `Text` to a QR Code using Error Correction level defined by `Level`. Response is a text string where the QR Code is encoded using block characters. | [Example][QrExample] |
| `QrEncode(Text,Level[,Width[,Height]])`            | Encodes text in `Text` to a QR Code using Error Correction level defined by `Level`. Response is a bitmap with the specified dimensions. If only `Width` is specified, `Height` will be assued to be identical. | [Example][QrExample2] |
| `SaveFile(Obj,FileName)`                           | Encodes an object `Obj` in accordance with its type and file extension, and saves it as a file. | [Example][SaveFileExample] |
| `UrlDecode(s)`                                     | Decodes a string taken from an URL. | [Example][UrlDecodeExample] |
| `UrlEncode(s)`                                     | Encodes a string for inclusion in an URL. | [Example][UrlEncodeExample] |
| `Utf8Decode(s)`                                    | Gets a string from UTF-8 encoded data. | [Example][Utf8DecodeExample] |
| `Utf8Encode(s)`                                    | Encodes a string using UTF-8. | [Example][Utf8EncodeExample] |
| `XmlDecode(s)`                                     | Decodes a string taken from XML. It transforms `&lt;`, `&gt;`, `&amp;`, `&quot;` and `&apos;` to `<`, `>`, `&`, `"` and `'`  correspondingly. | [Example][XmlDecodeExample] |
| `XmlEncode(s)`                                     | Encodes a string for inclusion in XML. It transforms `<`, `>`, `&`, `"` and `'` to `&lt;`, `&gt;`, `&amp;`, `&quot;` and `&apos;` correspondingly. | [Example][XmlEncodeExample] |

[Base64DecodeExample]: Prompt.md?Expression=Decode(Base64Decode("SGVsbG8="),"text/plain")
[Base64EncodeExample]: Prompt.md?Expression=Base64Encode(Encode("Hello")[0])
[Base64UrlDecodeExample]: Prompt.md?Expression=Decode(Base64UrlDecode("SGVsbG8"),"text/plain")
[Base64UrlEncodeExample]: Prompt.md?Expression=Base64UrlEncode(Encode("Hello")[0])
[CustomEncodeExample]: Prompt.md?Expression=CustomEncode(Bin,"application/x-mytype")
[DecodeExample]: Prompt.md?Expression=Decode(Csv,%22text/csv%22)
[DeleteExample]: Prompt.md?Expression=Delete(%22URL%22,{})
[EncodeExample]: Prompt.md?Expression=Encode("Hello",[%22text/plain%22])
[FileAttributesExample]: Prompt.md?Expression=FileAttributes(%22FileName%22)
[FileCreationTimeExample]: Prompt.md?Expression=FileCreationTime(%22FileName%22)
[FileCreationTimeUtcExample]: Prompt.md?Expression=FileCreationTimeUtc(%22FileName%22)
[FileLastAccessTimeExample]: Prompt.md?Expression=FileLastAccessTime(%22FileName%22)
[FileLastAccessTimeUtcExample]: Prompt.md?Expression=FileLastAccessTimeUtc(%22FileName%22)
[FileLastWriteTimeExample]: Prompt.md?Expression=FileLastWriteTime(%22FileName%22)
[FileLastWriteTimeUtcExample]: Prompt.md?Expression=FileLastWriteTimeUtc(%22FileName%22)
[GetExample]: Prompt.md?Expression=Get(%22URL%22)
[HeadExample]: Prompt.md?Expression=Head(%22URL%22)
[HtmlAttributeEncodeExample]: Prompt.md?Expression=HtmlAttributeEncode(%22%3Ctag%3E%22)
[HtmlValueEncodeExample]: Prompt.md?Expression=HtmlValueEncode(%22%3Ctag%3E%22)
[JsonDecodeExample]: Prompt.md?Expression=JsonDecode("{'a':1,'b':2}")
[JsonEncodeExample]: Prompt.md?Expression=JsonEncode({'a':1,'b':2})
[LoadFileExample]: Prompt.md?Expression=LoadFile(%22FileName%22)
[PostExample]: Prompt.md?Expression=Post(%22URL%22,%22Data%22)
[PrettyJsonExample]: Prompt.md?Expression=PrettyJson(Generalize(Obj))
[PutExample]: Prompt.md?Expression=Put(%22URL%22,%22Data%22)
[QrExample]: Prompt.md?Expression=QrEncode("Hello World","Q")
[QrExample2]: Prompt.md?Expression=QrEncode("Hello World","Q",300)
[SaveFileExample]: Prompt.md?Expression=SaveFile(Graph,%22Graph.png%22)
[UrlDecodeExample]: Prompt.md?Expression=UrlDecode(%22Hello%2bWorld%22)
[UrlEncodeExample]: Prompt.md?Expression=UrlEncode(%22Hello%20World%22)
[Utf8DecodeExample]: Prompt.md?Expression=Utf8Decode(Base64Decode("SGVsbG8="))
[Utf8EncodeExample]: Prompt.md?Expression=Base64Encode(Utf8Encode(%22Hello%20World%22))
[XmlDecodeExample]: Prompt.md?Expression=XmlDecode(%22%26lt%3Btag%26gt%3B%22)
[XmlEncodeExample]: Prompt.md?Expression=XmlEncode(%22%3Ctag%3E%22)

#### Cryptography-related functions (Waher.Script.Cryptography)

The following functions are available in the `Waher.Script.Cryptography` library.

| Function                                                   | Description | Example |
|------------------------------------------------------------|-------------|---------|
| `Aes256Decrypt(Content,Key,IV[,CipherMode[,PaddingMode]])` | Decrypts `Content` using AES 256, with the key `Key` and Initiation Vector `IV`. `CipherMode` is by default `CBC` and Padding is by default `PKCS7`. | [Example][Aes256DecryptExample] |
| `Aes256Encrypt(Content,Key,IV[,CipherMode[,PaddingMode]])` | Encrypts `Content` using AES 256, with the key `Key` and Initiation Vector `IV`. `CipherMode` is by default `CBC` and Padding is by default `PKCS7`. | [Example][Aes256EncryptExample] |
| `Ed25519([PrivKey[,HashKey]])`                             | Creates an Edwards25519 curve, with optional private key (`PrivKey`) and option to pre-hash key (`HashKey`). | [Example][Ed25519Example] |
| `Ed448([PrivKey[,HashKey]])`                               | Creates an Edwards448 curve, with optional private key (`PrivKey`) and option to pre-hash key (`HashKey`). | [Example][Ed448Example] |
| `HexDecode(Data)`                                          | Decodes binary data from a string using hexadecimal encoding. | [Example][HexDecodeExample] |
| `HexEncode(Data[,NrBytes])`                                | Encodes binary data to a string using hexadecimal encoding. Data can be an integer, if a second argument provides the number of bytes to use when encoding integer. | [Example][HexEncodeExample] |
| `Md5(Data)`                                                | Calculates a MD5 Hash Digest of the binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Md5Example] |
| `P192([PrivKey])`                                          | Creates a NIST-P192 curve, with optional private key (`PrivKey`). | [Example][P192Example] |
| `P224([PrivKey])`                                          | Creates a NIST-P224 curve, with optional private key (`PrivKey`). | [Example][P224Example] |
| `P256([PrivKey])`                                          | Creates a NIST-P256 curve, with optional private key (`PrivKey`). | [Example][P256Example] |
| `P384([PrivKey])`                                          | Creates a NIST-P384 curve, with optional private key (`PrivKey`). | [Example][P384Example] |
| `P521([PrivKey])`                                          | Creates a NIST-P521 curve, with optional private key (`PrivKey`). | [Example][P521Example] |
| `RandomBytes(NrBytes)`                                     | Generates an array of `N` random bytes. | [Example][RandomBytesExample] |
| `Sha1(Data)`                                               | Calculates a SHA-1 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha1Example] |
| `Sha1HMac(Data,Key)`                                       | Calculates a SHA-1 HMAC Hash Digest of `Data` using the key `Key`.  | [Example][Sha1HmacExample] |
| `Sha2_256(Data)`                                           | Calculates a 256-bit SHA-2 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha2256Example] |
| `Sha2_256HMac(Data,Key)`                                   | Calculates a 256-bit SHA-2 HMAC Hash Digest of `Data` using the key `Key`.  | [Example][Sha2256HmacExample] |
| `Sha2_384(Data)`                                           | Calculates a 384-bit SHA-2 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha2384Example] |
| `Sha2_384HMac(Data,Key)`                                   | Calculates a 384-bit SHA-2 HMAC Hash Digest of `Data` using the key `Key`.  | [Example][Sha2384HmacExample] |
| `Sha2_512(Data)`                                           | Calculates a 512-bit SHA-2 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha2512Example] |
| `Sha2_512HMac(Data,Key)`                                   | Calculates a 512-bit SHA-2 HMAC Hash Digest of `Data` using the key `Key`.  | [Example][Sha2512HmacExample] |
| `Sha3_224(Data)`                                           | Calculates a 224-bit SHA-3 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha3224Example] |
| `Sha3_256(Data)`                                           | Calculates a 256-bit SHA-3 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha3256Example] |
| `Sha3_384(Data)`                                           | Calculates a 384-bit SHA-3 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha3384Example] |
| `Sha3_512(Data)`                                           | Calculates a 512-bit SHA-3 Hash Digest of binary `Data`. If `Data` is a string, it is UTF-8 encoded first.  | [Example][Sha3512Example] |
| `Shake128(Bits,Data)`                                      | Calculates a SHAKE128 (part of SHA-3) Hash Digest (having `Bits` number of bits) of `Data`.  | [Example][Shake128Example] |
| `Shake256(Bits,Data)`                                      | Calculates a SHAKE256 (part of SHA-3) Hash Digest (having `Bits` number of bits) of `Data`.  | [Example][Shake256Example] |
| `X25519([PrivKey])`                                        | Creates a Curve25519 curve, with optional private key (`PrivKey`). | [Example][X25519Example] |
| `X448([PrivKey])`                                          | Creates a Curve448 curve, with optional private key (`PrivKey`). | [Example][X448Example] |

[Aes256EncryptExample]: Prompt.md?Expression=Data%3A%3DUtf8Encode%28%22Hello%20World%22%29%3BKey%3A%3DRandomBytes%2816%29%3BIV%3A%3DRandomBytes%2816%29%3BEncrypted%3A%3DAes256Encrypt%28Data%2CKey%2CIV%2C%22CBC%22%2C%22PKCS7%22%29
[Aes256DecryptExample]: Prompt.md?Expression=Decrypted%3A%3DAes256Decrypt%28Encrypted%2CKey%2CIV%2C%22CBC%22%2C%22PKCS7%22%29%3BUtf8Decode%28Decrypted%29
[Ed25519Example]: Prompt.md?Expression=Ed25519()
[Ed448Example]: Prompt.md?Expression=Ed448()
[HexDecodeExample]: Prompt.md?Expression=Utf8Decode(HexDecode("48656c6c6f"))
[HexEncodeExample]: Prompt.md?Expression=HexEncode(Utf8Encode("Hello"))
[Md5Example]: Prompt.md?Expression=Md5(Utf8Encode(%22Hello%22))
[P192Example]: Prompt.md?Expression=P192()
[P224Example]: Prompt.md?Expression=P224()
[P256Example]: Prompt.md?Expression=P256()
[P384Example]: Prompt.md?Expression=P384()
[P521Example]: Prompt.md?Expression=P521()
[RandomBytesExample]: Prompt.md?Expression=Base64Encode(RandomBytes(128))
[Sha1Example]: Prompt.md?Expression=Sha1(Utf8Encode(%22Hello%22))
[Sha1HmacExample]: Prompt.md?Expression=Sha1HMac(Utf8Encode(%22Hello%22),Utf8Encode(%22World%22))
[Sha2256Example]: Prompt.md?Expression=Sha2_256(Utf8Encode(%22Hello%22))
[Sha2256HmacExample]: Prompt.md?Expression=Sha2_256HMac(Utf8Encode(%22Hello%22),Utf8Encode(%22World%22))
[Sha2384Example]: Prompt.md?Expression=Sha2_384(Utf8Encode(%22Hello%22))
[Sha2384HmacExample]: Prompt.md?Expression=Sha2_384HMac(Utf8Encode(%22Hello%22),Utf8Encode(%22World%22))
[Sha2512Example]: Prompt.md?Expression=Sha2_512(Utf8Encode(%22Hello%22))
[Sha2512HmacExample]: Prompt.md?Expression=Sha2_512HMac(Utf8Encode(%22Hello%22),Utf8Encode(%22World%22))
[Sha3224Example]: Prompt.md?Expression=Sha3_224(Utf8Encode(%22Hello%22))
[Sha3256Example]: Prompt.md?Expression=Sha3_256(Utf8Encode(%22Hello%22))
[Sha3384Example]: Prompt.md?Expression=Sha3_384(Utf8Encode(%22Hello%22))
[Sha3512Example]: Prompt.md?Expression=Sha3_512(Utf8Encode(%22Hello%22))
[Shake128Example]: Prompt.md?Expression=Shake128(16*8,Utf8Encode(%22Hello%22))
[Shake256Example]: Prompt.md?Expression=Shake256(16*8,Utf8Encode(%22Hello%22))
[X25519Example]: Prompt.md?Expression=X25519()
[X448Example]: Prompt.md?Expression=X448()

#### XML-related functions (Waher.Script.Xml)

The following functions are available in the `Waher.Script.Xml` library.

| Function                         | Description                                                                                                                                                                                                | Pattern Matching          | Example                                    |
|----------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------|--------------------------------------------|
| `GetAttribute(Xml,Name)`         | Extracts the value of XML attribute `Name` from the element `Xml`. If not found, the empty string is returned. If `Xml` is null, `null` is returned.                                                       |                           | `GetAttribute(E,"name")`                   |
| `GetElement(Xml,Name)`           | Extracts the first child element to `Xml` with the given name `Name`. If not found, `null` is returned. If `Xml` is null, `null` is returned.                                                              |                           | `GetElement(E,"Name")`                     |
| `HasAttribute(Xml,Name)`         | Checks if an element `Xml` has an XML attribute `Name`. If `Xml` is null, `null` is returned.                                                                                                              |                           | `GetAttribute(E,"name")`                   |
| `HasElement(Xml,Name)`           | Checks if an element `Xml` has a child element with the given name `Name`. If `Xml` is null, `null` is returned.                                                                                           |                           | `GetElement(E,"Name")`                     |
| `InnerText(Xml)`                 | Returns the Inner Text of the XML Node `Xml`. If `Xml` is null, `null` is returned.                                                                                                                        |                           | `InnerText(E)`                             |
| `InnerXml(Xml)`                  | Returns the Inner XML of the XML Node `Xml`. If `Xml` is null, `null` is returned.                                                                                                                         |                           | `InnerXml(E)`                              |
| `OuterXml(Xml)`                  | Returns the Outer XML of the XML Node `Xml`. If `Xml` is null, `null` is returned.                                                                                                                         |                           | `OuterXml(E)`                              |
| `OwnerDocument(Xml)`             | Returns the XML Document owning the XML Node `Xml`. If `Xml` is null, `null` is returned.                                                                                                                  |                           | `OwnerDocument(E)`                         |
| `PrettyXml(Xml)`                 | Makes XML defined in `Xml` pretty, for display. If `Xml` is null, `null` is returned.                                                                                                                      |                           | `PrettyXml("<a><b>Hello</b></a>")`         |
| `SelectXml(Xml,XPath)`           | Performs an XPATH selection into `Xml`. The `default` prefix can be used to reference any default namespace in the XML. Results are returned as numbers if possible. If `Xml` is null, `null` is returned. |                           | `SelectXml(<a><b>Hello</b></a>,"/a/b")`    |
| `SelectXmlStr(Xml,XPath)`        | Performs an XPATH selection into `Xml` and returns the result as a literal string. The `default` prefix can be used to reference any default namespace in the XML. If `Xml` is null, `null` is returned.   |                           | `SelectXmlStr(<a><b>Hello</b></a>,"/a/b")` |
| `Xml(s)`                         | Converts the string `s` to an XML Document. If `s` is null, `null` is returned.                                                                                                                            | Asserts `s` is valid XML. | `Xml("<a>Hello</a>")`                      |

**Note**: The XML functions are made available so that access to elements and attributes in XML documents can be made accessible in
environments where the period (`.`) operator is not available, for security reasons.

#### XMLDSIG-related functions (Waher.Script.XmlDSig)

The following functions are available in the `Waher.Script.XmlDSig` library.

| Function                                 | Description | Example |
|------------------------------------------|-------------|---------|
| `RsaPublicKeyXml(KeyName,KeySize)`       | Exports the public key corresponding to an RSA key with name `KeyName` and size `KeySize`, in bits, as an XML document. | `RsaPublicKeyXml("Test",4096)` |
| `RsaPublicKey(Xml)`                      | Imports an RSA public key from its XML representation in `Xml`.                                                         | `RsaPublicKey(Xml)` |
| `SignXml(KeyName,KeySize,XML)`           | Signs XML using the XMLDSIG standard, given an RSA key with name `KeyName` and size `KeySize`, in bits.                 | `SignXml("Test",4096,<Test><a>1</a><b>2</b></Test>)` |
| `VerifyXml(KeyName,KeySize,XML)`         | Verifies signed XML using the XMLDSIG standard, given an RSA key with name `KeyName` and size `KeySize`, in bits.       | `VerifyXml("Test",4096,Xml)` |
| `VerifyXml(PublicKey,XML)`               | Verifies signed XML using the XMLDSIG standard, given an RSA public key, either parsed, or in XML.                      | `VerifyXml(PublicKey,Xml)` |

#### Markdown-related functions (Waher.Content.Markdown)

The following functions are available in the `Waher.Content.Markdown` library.

| Function                           | Description | Example |
|------------------------------------|-------------|---------|
| `CssContent(s)`                    | Encodes a string as a CSS Content object for encoding, as results of web service calls. | [Example][CssContentExample] |
| `FromMarkdown(Markdown)`           | Converts a string containing Markdown Representation to a script object. | [Example][FromMarkdownExample] |
| `HtmlContent(s)`                   | Encodes a string as an HTML Content object for encoding, as results of web service calls. | [Example][HtmlContentExample] |
| `InitScriptFile(FileName)`         | Evaluates the script in the file defined by `FileName` if not evaluated before, or if timestamp is newer than previous evaluation. | [Example][InitScriptFileExample] |
| `JavaScriptContent(s)`             | Encodes a string as a JavaScript Content object for encoding, as results of web service calls. | [Example][JavaScriptContentExample] |
| `LoadMarkdown(FileName[,Headers])` | Loads a markdown file and preprocesses it before returning it as a string. By default, Markdown headers are removed. If you wish Markdown headers to be included, set `Headers` to `true`. | [Example][LoadMarkdownExample] |
| `MarkdownContent(s)`               | Encodes a string as a Markdown Content object for encoding, as results of web service calls. | [Example][MarkdownContentExample] |
| `MarkdownEncode(s)`                | Encodes a string for inclusion in Markdown. | [Example][MarkdownEncodeExample] |
| `MarkdownToHtml(s)`                | Parses the markdown provided in `s` and converts it to HTML. Only HTML between BODY tags is returned. | [Example][MarkdowntoHtmlExample] |
| `MarkdownToHtmlStat(s)`            | Parses the markdown provided in `s` and converts it to HTML. Only HTML between BODY tags is returned. A vector containing the HTML, together with statistics about the document is returned. | [Example][MarkdowntoHtmlStatExample] |
| `MarkdownStatistics(s)`            | Parses the markdown provided in `s` and returns statistics about the document. | [Example][MarkdownStatisticsExample] |
| `ParseMarkdown(MD)`                | Parses the markdown provided in the string `MD`, and returns it as `MarkdownDocument` object. | [Example][ParseMarkdownExample] |
| `PreprocessMarkdown(MD)`           | Preprocesses a markdown string `MD`, and returns it as a string. | [Example][PreprocessMarkdownExample] |
| `ScriptFile(FileName)`             | Evaluates the script in the file defined by `FileName`. | [Example][ScriptFileExample] |
| `TextUnit(Nr,Sing,Plur)`           | Selects the singular (`Sing`) or plural (`Plur`) form of a unit to be used in text, based on the associated number (`Nr`). | [Example][ScriptTextUnitExample] |
| `ToMarkdown(Obj)`                  | Converts the object to a string containing the Markdown Representation of the object. | [Example][ToMarkdownExample] |

The following context-specific constants (read-only variables) are available in inline script:

| Variable        | Description                                                           |
|:---------------:|-----------------------------------------------------------------------|
| `StartPosition` | The starting position of the current script in the markdown document. |
| `EndPosition`   | The ending position of the script in the markdown document.           |

[CssContentExample]: Prompt.md?Expression=CssContent(%22CSS content%22)
[HtmlContentExample]: Prompt.md?Expression=HtmlContent(%22%3Cp%3EHTML+content%3C%2Fp%3E%22)
[JavaScriptContentExample]: Prompt.md?Expression=JavaScriptContent(%22javascript content%22)
[LoadMarkdownExample]: Prompt.md?Expression=LoadMarkdown(%22File.md%22)
[MarkdownContentExample]: Prompt.md?Expression=MarkdownContent(%22*markdown content*%22)
[MarkdownEncodeExample]: Prompt.md?Expression=MarkdownEncode(%22test_markdown%22)
[MarkdowntoHtmlExample]: Prompt.md?Expression=MarkdownToHtml(%22:file_folder:%22)
[MarkdowntoHtmlStatExample]: Prompt.md?Expression=MarkdownToHtmlStat(%22:file_folder:%22)
[MarkdownStatisticsExample]: Prompt.md?Expression=MarkdownStatistics(%22:file_folder:%22)
[ParseMarkdownExample]: Prompt.md?Expression=s%3A%3D%22Hello%20World%21%22%3BParseMarkdown%28%22%2A%7B%7Bs%7D%7D%2A%22%29
[PreprocessMarkdownExample]: Prompt.md?Expression=s%3A%3D%22Hello%20World%21%22%3BPreprocessMarkdown%28%22%2A%7B%7Bs%7D%7D%2A%22%29
[FromMarkdownExample]: Prompt.md?Expression=FromMarkdown(%22`10%20kWh`%22)
[ToMarkdownExample]: Prompt.md?Expression=ToMarkdown((x:=-10..10;y:=sin(x);plot2dcurve(x,y)))
[ScriptFileExample]: Prompt.md?Expression=ScriptFile(%22Example.script%22)
[InitScriptFileExample]: Prompt.md?Expression=InitScriptFile(%22InitExample.script%22)
[ScriptTextUnitExample]: Prompt.md?Expression=TextUnit(NrDays,"day","days")

#### Semantic-web-related functions (Waher.Content.Semantic)

The following functions are available in the `Waher.Content.Semantic` library.

| Function             | Description                                                                                           |
|----------------------|-------------------------------------------------------------------------------------------------------|
| `BNode([Label])`     | Creates a blank node with a given label. If no label is provided, a distinct blank node is created.   |
| `Coalesce(Exp, ...)` | Returns the value of the first expression in the argument list, that evaluates without error.         |
| `DataType(Term)`     | Returns the data type of the term, if available, or the empty string if not.                          |
| `IsBlank(Term)`      | Checks if an RDF term is a Blank node.                                                                |
| `IsIri(Term)`        | Alias for `IsUri`.                                                                                    |
| `IsLiteral(Term)`    | Checks if an RDF term is a literal.                                                                   |
| `IsNumeric(Term)`    | Checks if an RDF term is a numeric literal.                                                           |
| `IsTriple(Term)`     | Checks if an RDF term is a quoted semantic triple.                                                    |
| `IsUri(Term)`        | Checks if an RDF term is a URI term.                                                                  |
| `Lang(Term)`         | Returns the language of the term, if available, or the empty string if not.                           |
| `LangMatches(x,y)`   | Checks if the language `x` matches the language `y`. `y` can be `*`, or unlocalized, while `x` is.    |
| `Object(Triple)`     | Returns the object part of a semantic triple.                                                         |
| `Predicate(Triple)`  | Returns the predicate part of a semantic triple.                                                      |
| `StrDt(x,Type)`      | Returns a typed literal, with XML data type specified by `Type`, and string value represented by `x`. |
| `StrLang(x,Lang)`    | Returns a localized literal, with language specified by `Lang`, and string value represented by `x`.  |
| `Subject(Triple)`    | Returns the subject part of a semantic triple.                                                        |
| `TimeZone(x)`        | Returns the time zone of a date and time literal value, as a `xs:dayTimeDuration` literal value.      |
| `Triple(s,p,o)`      | Returns a semantic triple, given the subject (`s`), predicate (`p`) and object (`o`).                 |
| `Tz(x)`              | Returns the time zone of a date and time literal value, as a string literal value.                    |

#### XSL-related functions (Waher.Content.Xsl)

The following functions are available in the `Waher.Content.Xsl` library.

| Function                 | Description | Example |
|--------------------------|-------------|---------|
| `Transform(XML,XSLT)`    | Transforms an XML document using an XSL Transform (XSLT). | [Example][TransformExample] |

[TransformExample]: Prompt.md?Expression=Transform(LoadFile(%22Data.xml%22),LoadFile(%22Transform.xslt%22))

#### Layout Extensions (Waher.Layout.Layout2D)

The following functions are available in the `Waher.Layout.Layout2D` library.

| Function                                  | Description | Example |
|-------------------------------------------|-------------|---------|
| `Layout(Xml)`                             | Creates a bitmapped graph from a layout. The Layout can be an XML Document, XML Element or XML as a string. | `Layout(Xml)`             |
| `Legend(Labels,Colors,FgColor,NrColumns)` | Creates a legend that can be displayed in association with a graph containing multiple series.              | `Legend(Labels,Colors,4)` |

In the following subsections, specialized HTTP Error functions are listed.

#### Web Extensions (Waher.Networking.HTTP\[.UWP\])

Script can be embeded in transformable web content, such as [Markdown documents](/Markdown.md#script). The following functions are 
available in the `Waher.Networking.HTTP` and `Waher.Networking.HTTP.UWP` libraries.

| Function                                                        | Description | Example |
|-----------------------------------------------------------------|-------------|---------|
| `EphemeralUser(UserName[,JID[,Privileges[,Properties]]])`       | Creates an ephemeral user that is not persisted. The user object will have a given user name, an optional JID, an optional vector of permissions granted, and an optional object ex-nihilo of additional properties that may be used in the application. | `EphemeralUser("Testuser","testuser@example.org",["Tests\..*"],{"a":1,"b":2})` |
| `HttpError(Code,Message,Content)`                               | Returns an HTTP Error to the client y throwing an `HttpException` exception. | `HttpError(400,"Bad Request","Missing parameters.")` |
| `AuthenticateBasic(Request,Realm,Users[,EncStrength])`          | Performs basic authentication of the user of the request. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` can be a boolean (for if encryption is required yes/no) or a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateBasic(Request,Gateway.Domain,Waher.Security.Users.Users.Source,128)` |
| `AuthenticateDigestMd5(Request,Realm,Users[,EncStrength])`      | Performs DIGEST MD5 authentication of the user of the request. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` can be a boolean (for if encryption is required yes/no) or a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateDigestMd5(Request,Gateway.Domain,Waher.Security.Users.Users.Source,128)` |
| `AuthenticateDigestSha256(Request,Realm,Users[,EncStrength])`   | Performs DIGEST SHA256 authentication of the user of the request. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` can be a boolean (for if encryption is required yes/no) or a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateDigestSha256(Request,Gateway.Domain,Waher.Security.Users.Users.Source,128)` |
| `AuthenticateDigestSha3_256(Request,Realm,Users[,EncStrength])` | Performs DIGEST SHA3-256 authentication of the user of the request. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` can be a boolean (for if encryption is required yes/no) or a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateDigestSha3_256(Request,Gateway.Domain,Waher.Security.Users.Users.Source,128)` |
| `AuthenticateMutualTls(Request,Users[,EncStrength])`            | Performs a Mutual TLS authentication of the user of the request. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` is a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateMutualTls(Request,Waher.Security.Users.Users.Source,128)` |
| `AuthenticateSession(Request,UserVariable[,EncStrength])`       | Performs a authentication of the user of the request, by checking the value of a user variable in the current session. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` is a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateSession(Request,"User",128)` |
| `Authorize(User,Privileges[,Message])`                          | Checks if a user is authorized access by checking it has all privileges provided in `Privileges`. If `Message` is provided, it will be used as exception message is user is not authorized. | `Authorize(User,"App.Module.Function","Access denied.")` |

In the following subsections, specialized HTTP Error functions are listed.

##### Redirections

The following functions return HTTP redirection responses back to he client:

| Function                      | Code | Description |
|-------------------------------|-----:|-------------|
| `Found(Location)`             |  302 | Returns the Found redirection back to the client.              |
| `MovedPermanently(Location)`  |  301 | Returns the Moved Permanently redirection back to the client.  |
| `NotModified()`               |  304 | Returns the Not Modified message back to the client.           |
| `PermanentRedirect(Location)` |  308 | Returns the Permanent Redirect redirection back to the client. |
| `SeeOther(Location)`          |  303 | Returns the See Other redirection back to the client.          |
| `TemporaryRedirect(Location)` |  307 | Returns the Temporary Redirect redirection back to the client. |
| `UseProxy(Location)`          |  305 | Returns the Use Proxy redirection back to the client.          |

##### Client Errors

The following functions return HTTP client error responses back to he client:

| Function                              | Code | Description |
|---------------------------------------|-----:|-------------|
| `BadRequest(Content)`                 |  400 | Returns the Bad Request client error message back to the client.                   |
| `Conflict(Content)`				    |  409 | Returns the Conflict client error message back to the client.                      |
| `FailedDependency(Content)`		    |  424 | Returns the Failed Dependency client error message back to the client.             |
| `Forbidden(Content)`                  |  403 | Returns the Forbidden client error message back to the client.                     |
| `Gone(Content)`					    |  410 | Returns the Gone client error message back to the client.                          |
| `Locked(Content)`				        |  423 | Returns the Locked client error message back to the client.                        |
| `MisdirectedRequest(Content)`	        |  421 | Returns the Misdirected Request client error message back to the client.           |
| `NotAcceptable(Content)`			    |  406 | Returns the Not Acceptable client error message back to the client.                |
| `NotFound(Content)`				    |  404 | Returns the Not Found client error message back to the client.                     |
| `PreconditionFailed(Content)`	        |  411 | Returns the Precondition Failed client error message back to the client.           |
| `PreconditionRequired(Content)`	    |  428 | Returns the Precondition Required client error message back to the client.         |
| `RangeNotSatisfiable(Content)`	    |  416 | Returns the Range Not Satisfiable client error message back to the client.         |
| `RequestTimeout(Content)`		        |  408 | Returns the Request Timeout client error message back to the client.               |
| `TooManyRequests(Content)`		    |  429 | Returns the Too Many Requests client error message back to the client.             |
| `UnavailableForLegalReasons(Content)` |  451 | Returns the Unavailable For Legal Reasons client error message back to the client. |
| `UnprocessableEntity(Content)`	    |  422 | Returns the Unprocessable Entity client error message back to the client.          |
| `UnsupportedMediaType(Content)`       |  415 | Returns the Unsupported Media Type client error message back to the client.        |

##### Server Errors

The following functions return HTTP server error responses back to he client:

| Function                                 | Code | Description |
|------------------------------------------|-----:|-------------|
| `BadGateway(Content)`                    |  502 | Returns the Bad Gateway server error message back to the client.                     |
| `GatewayTimeout(Content)`                |  504 | Returns the Gateway Timeout server error message back to the client.                 |
| `InsufficientStorage(Content)`           |  507 | Returns the Insufficient Storage server error message back to the client.            |
| `InternalServerError(Content)`           |  500 | Returns the Internal Server Error server error message back to the client.           |
| `LoopDetected(Content)`                  |  508 | Returns the Loop Detected server error message back to the client.                   |
| `NetworkAuthenticationRequired(Content)` |  511 | Returns the Network Authentication Required server error message back to the client. |
| `NotExtended(Content)`                   |  510 | Returns the Not Extended server error message back to the client.                    |
| `NotImplemented(Content)`                |  501 | Returns the Not Implemented server error message back to the client.                 |
| `ServiceUnavailable(Content)`            |  503 | Returns the Service Unavailable server error message back to the client.             |
| `VariantAlsoNegotiates(Content)`         |  506 | Returns the Variant Also Negotiates server error message back to the client.         |

##### Special Web Variables

The following predefined variables or constants are available when processing web requests:

| Variable   | Description                              |
|:----------:|------------------------------------------|
| `Request`  | The current HttpRequest object.          |
| `Response` | The current HttpResponse object.         |
| `Posted`   | Any decoded data posted to the resource. |
| `Global`   | Global variables.                        |
| `Page`     | Page-local variables.                    |

#### JWS Extensions (Waher.Security.JWS)

The following functions are available in the `Waher.Security.JWS` librariy.

| Function          | Description                                                  | Example                                  |
|-------------------|--------------------------------------------------------------|------------------------------------------|
| `HS256([Secret])` | Creates an HMAC SHA-256 JSON Web Signature key.              | `HS256(Utf8Encode("Hello world"))`       |
| `RS256([Rsa])`    | Creates an RSASSA-PKCS1-v1_5 SHA-256 JSON Web Signature key. | `RS256(RsaFromPem(Account.private_key))` |

#### JWT Extensions (Waher.Security.JWT\[.UWP\])

The following functions are available in the `Waher.Security.JWT` and `Waher.Security.JWT.UWP` libraries.

| Function                                                       | Description | Example |
|----------------------------------------------------------------|-------------|---------|
| `AuthenticateJwt(Request,Realm,Users[,EncStrength][,Factory])` | Performs Bearer and JWT authentication of the user of the request. If user is not authenticated, and appropriate exception is thrown and error returned to the client. If authentication is performed, authenticated user object is returned. `EncStrength` can be a boolean (for if encryption is required yes/no) or a positive integer value, mening the minimum cipher strength requried for the mechanism. An optional JWT Factory can be provided, unless the default system JWT Factory is to be used. | `AuthenticateJwt(Request,Gateway.Domain,Waher.Security.Users.Users.Source,128)` |
| `CreateJwt(Claims[,Factory])`                                  | Returns a Java Web Token (JWT) generated from a set of claims. Claims must be expressed as an object ex-nihilo, where claim names are property names, and values are the corresponding values. An optional JWT Factory can be provided, unless the default system JWT Factory is to be used. | `Token:=CreateJwt({"iss":"www.example.com","name":"Jon Doe"})` |
| `CreateJwtFactory(Algorithm)`                                  | Creates a JWT Factory using a specific JWS algorithm. | `Factory:=CreateJwtFactory(RS256(Account.private_key))` |
| `ValidateJwt(Token[,Factory])`                                 | Validates a Java Web Token (JWT) generated by `CreateJwt`. If not valid, an exception is thrown. If valid, the parsed token is returned. Claims are available through its `Claims` property. An optional JWT Factory can be provided, unless the default system JWT Factory is to be used. | `ValidateJwt(Token)` |

#### Thing-related Extensions (Waher.Things)

The following functions are available in the `Waher.Things` library.

| Function                                                    | Description |
|-------------------------------------------------------------|-------------|
| `AddableTypes(Node)`                                        | Gets an array of types of nodes that can be added to an existing node. |
| `Field(Thing,Name,Value)`                                   | Creates a field object depending on the type of the value (Momentary Value, Automatic Readout, not writable) using the current time. |
| `Field(Thing,Timestamp,Name,Value[,Type[,QoS[,Writable]]])` | Creates a field object depending on the type of the value. If Type is omitted, it is assumed the field is a Momentary Value. If QoS is omitted, it is assumed it's a Momentary Readout. If is also assumed the field is not writable, unless declared otherwise. |
| `ThingReference([NodeId[,SourceId[,Partition]]])`           | Creates a reference object pointing to a node on the gateway. |

#### Gateway Extensions (Waher.IoTGateway)

The following functions are available in web pages hosted by the IoT Gateway:

| Function                                              | Description |
|-------------------------------------------------------|-------------|
| `BareJID(JID)`                                        | Returns the Bare JID of `JID` |
| `ClearCaches()`                                       | Clears internal caches. |
| `FullJID(JID)`                                        | Returns the Full JID of `JID`. If `JID` is a Bare JID, the Full JID of the last online presence is returned. |
| `GetDomainSetting(Host,Key,DefaultValue)`             | Gets a domain setting. If the Host (which can be a string or implement `Waher.Content.IHostReference`, such as an HTTP Request object for instance) is an alternative domain, it will be treated as a host setting, otherwise a runtime setting. |
| `GetNode(NodeId[,SourceId[,Partition[,JID]]])`        | Gets the node object of a node in the gateway (if not providing a `JID`), or a provisional reference node to a node hosted by a remote gateway identified by `JID`. If the node is not found, null is returned. (If no Source ID is provided, the Metering Topology is assumed.) Authorization to view the node is required, and depends on the execution context. |
| `GetSources()`                                        | Gets available sources of things. |
| `GetTabIDs([Page[,QueryFilter]])`                     | Gets an array of open tabs (as string TabIDs) loading the `Events.js` javascript file. Pages can be optionally restricted to a given `Page`, and optionally further restricted by a query filter, as an [object ex-nihilo](#objectExNihilo) specifying query parameters and values. |
| `GetTabIDs(Pages)`                                    | Gets an array of open tabs (as string TabIDs) loading the `Events.js` javascript file. Tabs returned must be showing any of the pages provied in the vector `Pages`. |
| `GetTabIDs(User)`                                     | Gets an array of open tabs (as string TabIDs) loading the `Events.js` javascript file. Tabs returned must be viewed by the user identitied by the user object `User`. |
| `GetTabInformation(TabID)`                            | Gets information about a tab, the URI used, query parameters, session ID and session variables. If tab is not found, `null` is returned. |
| `LoadResourceFile(LocalResource[,ContentType])`       | Loads a file from its local resource name and decodes it, taking into consideration defined web folders. By default, the content type defined by the file extension is used, if defined. You can also explicitly provide a content type. |
| `PreprocessCssx(CSSX)`                                | Preprocesses a CSSX string `CSSX`, and returns it as a string. |
| `PreprocessHtmlx(HTMLX)`                              | Preprocesses a HTMLX string `HTMLX`, and returns it as a string. |
| `PushEvent(..., Event, Data)`                         | Pushes an event to all open pages (tabs), defined by the arguments defined by `...` (same types of arguments as for the `GetTabIDs` function), or a reference to a Tab ID. Data can be a string, or any object that can be encoded as JSON. The `Event` translates to a JavaScript function, with one argument, that will be called. The `Data` will be passed on as the argument. |
| `RandomPassword()`                                    | Creates a random password having approximately 255 bits of entropy. |
| `ReadSensorData(Sensor[,Types[,Fields[,From[,To]]]])` | Reads a sensor defined on the gateway. Authorization to read the sensor depends on the execution context. The response is an object of the type `{"Error":boolean,"Ok":boolean,"Fields":Dictionary<string,Field>,"Errors":ThingError}` |
| `ReloadPage(...)`                                     | Reloads all open pages (tabs), defined by its arguments. The same types of arguments as for the `GetTabIDs` function can be used. |
| `RemoveScriptResource(Resource)`                      | Removes a script resource added using the `ScriptResource` function. The function returns a Boolean value showing if such a resource was found and removed. |
| `ResourceFileName(LocalResource)`                     | Returns the file name that corresponds to a local resource, taking into consideration defined web folders. |
| `ScriptResource(Resource,Expression)`                 | Defines a script resource on the web server hosted by the gateway. Script resources are persisted, and will be available after the gateway is restarted, until they are removed by calling `RemoveScriptResource`. If a non-script resource already exists with the given name, the new resource is not added. The function returns a Boolean value showing if the script resource was added or not. |
| `SetDomainSetting(Host,Key,Value)`                    | Sets a domain setting. If the Host (which can be a string or implement `Waher.Content.IHostReference`, such as an HTTP Request object for instance) is an alternative domain, it will be treated as a host setting, otherwise a runtime setting. |

The following predefined variables are available in web pages hosted by the IoT Gateway:

| Variable       | Description                                              |
|:--------------:|----------------------------------------------------------|
| `Domain`       | The domain on which the gateway is operating.            |
| `Language`     | The language object of the current session.              |
| `Namespace`    | The language namespace object of the current page.       |
| `Runtime`      | Returns the time elapsed since the gateway was started.  |
| `Started`      | Date and Time of when the gateway started.               |

#### Gateway Console Extensions (Waher.IoTGateway.Console)

The following functions are available in the `Waher.IoTGateway.Console` console application library. Running the IoT Gateway
as a console application, adds these functions to the script engine.

| Function              | Description                                            | Example |
|-----------------------|--------------------------------------------------------|---------|
| `RsaFromPem(PemFile)` | Creates an RSA object from the contents of a PEM file. | `RsaFromPem(PemContents)` |

#### Gateway Service Extensions (Waher.IoTGateway.Svc)

The following functions are available in the `Waher.IoTGateway.Svc` Windows Service host application library. Running the IoT Gateway
as a Windows Service, adds these functions to the script engine.

| Function                                                       | Description                                                                                                                                                                | Example |
|----------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------|
| `RsaFromPem(PemFile)`                                          | Creates an RSA object from the contents of a PEM file.                                                                                        | `RsaFromPem(PemContents)` |
| `DecPerformanceCounter(CategoryName[,Instance],CounterName)`   | Decrements a performance counter, given a performance category name, optional instance name, as well as a counter name.                                                    | `DecPerformanceCounter("My Counters","My Instance","My Counter")` |
| `IncPerformanceCounter(CategoryName[,Instance],CounterName)`   | Increments a performance counter, given a performance category name, optional instance name, as well as a counter name.                                                    | `IncPerformanceCounter("My Counters","My Instance","My Counter")` |
| `PerformanceCategory(CategoryName)`                            | Returns a `System.Diagnostics.PerformanceCounterCategory` object, given the category name.                                                                                 | `properties(PerformanceCategory("Processor"))` |
| `PerformanceCounter(CategoryName[,Instance],CounterName)`      | Returns a `System.Diagnostics.PerformanceCounter` object, given a performance category name and optional performance instance name, as well as a performance counter name. | `PerformanceCounter("Processor","_Total","% Processor Time")` |
| `PerformanceCounterValue(CategoryName[,Instance],CounterName)` | Returns a performance counter value, given a performance category name, optional instance name, as well as a counter name.                                                 | `PerformanceCounterValue("Processor","_Total","% Processor Time")` |
| `PerformanceCounters(CategoryName[,Instance])`                 | Returns an array of `System.Diagnostics.PerformanceCounter` objects within a given performance category, and optional performance instance.                                | `PerformanceCounters("Processor","_Total")` |
| `PerformanceCounterNames(CategoryName[,Instance])`             | Returns an array of names of performance counters within a given performance category, and optional performance instance.                                                  | `PerformanceCounterNames("Processor","_Total")` |
| `PerformanceInstances(CategoryName)`                           | Returns an array of performance counter instance names, given a performance category.                                                                                      | `PerformanceInstances("Processor")` |

The following predefined constants are also available:

| Variable                   | Description                                                                  |
|:--------------------------:|------------------------------------------------------------------------------|
| `PerformanceCategories`    | Returns an array of `System.Diagnostics.PerformanceCounterCategory` objects. |
| `PerformanceCategoryNames` | Returns an array of performance counter category names.                      |

#### Serialization-related functions (Waher.Service.NeuroLedger)

The following functions are available in the `Waher.Service.NeuroLedger` library, which is part of the Neuro-Ledger^TM.

| Function                            | Description                                                                                                                                                                                          | Example |
|-------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------|
| `Deserialize(Bin[,BaseType])`       | Deserializes a byte array into an object, or vector of objects. If serialization does not include type information, a base type can be provided. If not, a generic deserializer will be used.        | `Deserialize(Base64Decode(s))` |
| `FromBinary(Bin[,BaseType])`        | Alias for `Deserialize`.                                                                                                                                                                             | `FromBinary(Base64Decode(s))` |
| `Serialize(Object)`                 | Serializes an object to a byte array.                                                                                                                                                                | `Base64Encode(Serialize(Obj))` |
| `Serialize(Vector)`                 | Serializes a vector of objects to a byte array.                                                                                                                                                      | `Base64Encode(Serialize([Obj1,Obj2,Obj3]))` |
| `PrintDeserialize(Bin[,BaseType])`  | Works as `Deserialize(Bin[,BaseType])`, except that individual elements are printed to the standard output. This can be used for debugging purposes when transporting serializations across systems. | `PrintDeserialize(Base64Decode(s))` |
| `PrintSerialize(Object)`            | Works as `Serialize(Object)`, except that individual elements are printed to the standard output. This can be used for debugging purposes when transporting serializations across systems.           | `Base64Encode(PrintSerialize(Obj))` |
| `PrintSerialize(Vector)`            | Works as `Serialize(Vector)`, except that individual elements are printed to the standard output. This can be used for debugging purposes when transporting serializations across systems.           | `Base64Encode(PrintSerialize([Obj1,Obj2,Obj3]))` |
| `ToBinary(Object)`                  | Alias for `Serialize`.                                                                                                                                                                               | `Base64Encode(ToBinary(Obj))` |
| `ToBinary(Vector)`                  | Alias for `Serialize`.                                                                                                                                                                               | `Base64Encode(ToBinary([Obj1,Obj2,Obj3]))` |

#### IoT Broker Extensions (available in Waher.Service.IoTBroker)

The following subsections list functions that are available to applications running on the IoT Broker (hosting the `Waher.Service.IoTBroker` 
library) or the Neuron^TM.

##### Functions related to local services

| Function                                              | Description                                                                                                                                                                    | Example                                                            |
|-------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| `ConfigurationPage(Title,LocalResource[,Privileges])` | Defines a configuration page to display in the admin portal. Typically called as part of the setup and configuration phase of a configurable content-only module.              | `ConfigurationPage("Settings","/Svc/Cnf.md","Admin.Svc.Settings")` | 
| `IpLocale(IP)`                                        | Looks up locale information about an IP address.                                                                                                                               | `IpLocale("1.2.3.4")`                                              |
| `QuickLoginServiceId(Request[,AgentApiTimeout])`      | Generates a Quick-Login service ID, from an HTTP Request object. If the quick-login should generate an Agent API login at the same time, a timeout in seconds can be provided. | `QuickLoginServiceId(Request)`                                     |

##### E-mail related functions

| Function                                                              | Description                                                                                                                                                                                                                                                                 | Example                                      |
|-----------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------|
| `Attachment(Object,FileName)`                                         | Encodes an object for use as an attachment in multi-format encoded content.                                                                                                                                                                                                 | `Attachment(PngImage,"Photo.png")`           |
| `Attachment(EncodedObject,ContentType,FileName)`                      | Creates an attachment for multi-format encoded content, based on an encoded object.                                                                                                                                                                                         | `Attachment(PngRaw,"image/png","Photo.png")` |
| `Embed(Object,ContentId)`                                             | Encodes an object for use as an inline embedding in multi-format encoded content.                                                                                                                                                                                           | `embed(PngImage,"000001")`                   |
| `Embed(EncodedObject,ContentType,ContentId)`                          | Creates an inline embedding for multi-format encoded content, based on an encoded object.                                                                                                                                                                                   | `embed(PngRaw,"image/png","000001")`         |
| `SendMail([[Host,Port,User,Pwd,]From,]To,Subject,Markdown[,Objects])` | Sends an formatted e-mail to a recipient. If SMTP details are not provided, SMTP Relay settings are used by default. If such are not provided, broker will act as an SMTP relay itself. The `Objects` argument can be used to embed inline or attached objects to the mail. | `SendMail(To,Subject,Body)`                  |

##### Smart Contracts-related functions

| Function                                                               | Description                                                                                                                                                                                                            | Example                                                           |
|------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------|
| `CreateContract(Account,TemplateId[,Visibility[,Properties[,Parts]]])` | Creates a contract for an account on the broker, based on an existing contract template. `Visibility`, `Parts` and `Properties` can be set to new values. `Parts` and `Properties` are assumed to be object ex-nihilo. | `CreateContract(Account,Template,"Public",null,{A:10,B:20,C:30})` |
| `ObsoleteContract(ContractId)`                                         | Obsoletes a contract on the Neuron(R), given that the contract exists on the Neuron(R), and the script is executed from a session, with a user with sufficient privileges.                                             | `ObsoleteContract(ContractId)`                                    |
| `RejectContract(ContractId)`                                           | Rejects a contract on the Neuron(R), given that the contract exists on the Neuron(R), and the script is executed from a session, with a user with sufficient privileges.                                               | `RejectContract(ContractId)`                                      |
| `SendProposal(ContractId,Jid,Role,Proposal)`                           | Sends a contract proposal with ID `ContractId` to an XMPP client using address `JID`, for signing as `Role`. Message inclues a `Proposal` text.                                                                        | `SendProposal(ContractId,ContactJid,"Creator","Please sign.")`    |

##### Token-related functions

| Function                         | Description                                                                                                                                                                                                           | Example                                        |
|----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------|
| `CallAction(ActionRef)`          | Calls an action from inside a running state-machine.                                                                                                                                                                  | `CallAction('Action01')`                       |
| `HistoryReport()`                | Generates a history report, in Markdown format, from inside a running state-machine.                                                                                                                                  | `HistoryReport()`                              |
| `LoadAttachments([ContentTypes]) | Loads attachments associated with the definition contract. If a content types are provided (a single scalar, or a vector of content-types), only attachments having such types will be loaded. Wildcards can be used. | `LoadAttachments(["text/markdown","image/*"])` |
| `PresentReport()`                | Generates a present report, in Markdown format, from inside a running state-machine.                                                                                                                                  | `PresentReport()`                              |
| `ProfilingReport()`              | Generates a profiling report, in Markdown format, from inside a running state-machine.                                                                                                                                | `ProfilingReport()`                            |
| `StateDiagram()`                 | Generates a state diagram, in Markdown format, from inside a running state-machine.                                                                                                                                   | `StateDiagram()`                               |

##### XMPP-related functions

| Function                                                 | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Example                                                            |
|----------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| `AuthenticateXmppToken(Request[,Users][,EncStrength])`   | Performs Bearer and JWT authentication of a token generate for a connected XMPP client. If authentication is performed, authenticated user object is returned referring to the corresponding client connection. This funcion also authenticates the sender of an HTTPX (HTTP over XMPP) client requesting resources on the server, without the use of Bearer token. `EncStrength` can be a boolean (for if encryption is required yes/no) or a positive integer value, mening the minimum cipher strength requried for the mechanism. | `AuthenticateXmppToken(Request,128)`                               |
| `IqGet(To,Xml)`                                          | Performs an XMPP IQ (Information Query) Get operation.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | `IqGet(JID,<ping xmlns='urn:xmpp:ping'/>)`                         |
| `IqSet(To,Xml)`                                          | Performs an XMPP IQ (Information Query) Set operation.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | `IqSet(JID,<ping xmlns='urn:xmpp:ping'/>)`                         |
| `PingXmpp(To)`                                           | Performs an XMPP Ping operation and returns the number of milliseconds required for the roundtrip.                                                                                                                                                                                                                                                                                                                                                                                                                                    | `PingXmpp(JID)`                                                    |
| `PresenceProbe(BareJid)`                                 | Performs an XMPP Presence Probe on a Bare JID, to retrieve the latest presence sent by the account.                                                                                                                                                                                                                                                                                                                                                                                                                                   | `PresenceProbe("Account@server.com")`                              |
| `RosterName(LocalJid, RemoteJid)`                        | Returns the name of a contact in a local users roster. If no name is returned, the Bare JID is returned.                                                                                                                                                                                                                                                                                                                                                                                                                              | `RosterName(To,From)`                                              |
| `SendCustomMessage(To,Xml)`                              | Sends a custom XML XMPP message to a recipient `To`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | `SendCustomMessage(To,<hello xmlns='http://example.com/Kilroy'/>)` |
| `SendFormattedMessage(To,Markdown[,Subject[,Language]])` | Sends a multi-format XMPP message to a recipient `To`, with an optional `Subject` and `Language`. The [Markdown](/Markdown.md) provided will be transformed to plain text and HTML and included in the message as well, together with a regeneration of the Markdown provided, to improve interoperability. The recipient can decide what format to process or display.                                                                                                                                                               | `SendFormattedMessage(To,"*Hello* there.")`                        |
| `SendPlainMessage(To,Text[,Subject[,Language]])`         | Sends a plain-text XMPP message to a recipient `To`, with an optional `Subject` and `Language`.                                                                                                                                                                                                                                                                                                                                                                                                                                       | `SendPlainMessage(To,"Hello there.")`                              |

#### Multi-User Chat Extensions (available in Waher.Service.LilSis, Waher.Service.IoTBroker)

The following functions are available in applications using Multi-User Chat, such as the `Waher.Service.LilSis` library,
part of Lil'Sis'(R), and the `Waher.Service.IoTBroker` library, part of the Neuron^TM.

| Function                                                          | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Example                                                                                                                          |
|-------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| `Consolidate(RoomId,Domain,Command[Preview[,Config[,Variable]]])` | Sends a command `Command` to a MUC room identified by `RoomId` and `Domain`, and consolidates responses into one. `Preview` is a Boolean value, specifying if intermediate results should be previewed or not. `Config` can be one of three things: If a positive integer, it defines the number of expected responses, before consolidation is returned. If an array of strings, represents the nick-names of the sources that are expected to return responses. If an object ex-nihilo, it defining default responses from the expected sources. These default responses will be displayed until proper responses are returned. If no `Config` is provided, the function returns when all online occupants of the room have responded. `Variable` may contain a string containing the name of an optional variable that will contain the consolidator of the operation. It can be used to derive further information about the consolidated result after the function completes.  | `Consolidate("Room","example.com","select Type, Count(*) Nr from PersistedEvent where Timestamp>Now.AddDays(-1) group by Type")` |
| `Occupants(RoomId,Domain)`                                        | Returns an array of nick-names corresponding to occupants in a Multi-User Chat Room.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | `Occupants("Room","example.com"`                                                                                                 |

The following predefined constants are also available:

| Variable | Description                                                         |
|:--------:|---------------------------------------------------------------------|
| `MUC`    | Gives access to remote data sources hosted by neurons in MUC rooms. |

#### Payment Extensions (available in Paiwise)

The following functions are available in applications running on the Paiwise nuget. This includes the
IoT Broker or the Neuron^TM.

| Function                                                | Description                                                                                                           | Example                     |
|---------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------|-----------------------------|
| `ExchangeRate(From,To)`                                 | Gets the current exchange rate between two currencies. Requires a currency converter to be configured for the system. | `ExchangeRate("USD","EUR")` |
| `GetServiceProvidersForBuyingEDaler(Country,Currency)`  | Gets available service providers that can be used to buy eDaler(R) in a given country, given its country code, and a currency. | `GetServiceProvidersForBuyingEDaler("US","USD")` |
| `GetServiceProvidersForPayments(Country,Currency)`      | Gets available service providers that can be used to pay for services in a given country, given its country code, and a currency. | `GetServiceProvidersForPayments("US","USD")` |
| `GetServiceProvidersForPeerReview(MetaData)`            | Gets available service providers that can be used for peer reviews of identity applications. Meta-data about the person is provided in the `MetaData` argument, which is assumed to be an object ex-nihilo. | `GetServiceProvidersForPeerReview({"COUNTRY":"SE","CITY":"Stockholm"})` |
| `GetServiceProvidersForSellingEDaler(Country,Currency)` | Gets available service providers that can be used to sell eDaler(R) in a given country, given its country code, and a currency. | `GetServiceProvidersForSellingEDaler("US","USD")` |

#### Microsoft Interoperability (available in TAG.MicrosoftInterop.package)

The following functions are available on systems with the `TAG.MicrosoftInterop.package` installed. A more detailed description
about the script functions referenced in this section is available in the [MicrosoftInterop repository](https://github.com/Trust-Anchor-Group/MicrosoftInterop#converting-documents-in-script).

| Function              | Description                                           |
|-----------------------|-------------------------------------------------------|
| `WordToMarkdown(Doc)` | Converts a Word document to [Markdown](/Markdown.md). |
| `ExcelToScript(Doc)`  | Converts an Excel document to Script.                 |

#### Scriptable Providers Extensions (available in TAG.ScriptProviders.package)

The following functions are available on systems with the `TAG.ScriptProviders.package` installed. A more detailed description
about the script functions referenced in this section is available in the [NeuronScriptProviders repository](https://github.com/Trust-Anchor-Group/NeuronScriptProviders#creating-script-based-services).

| Function                         | Description                                           |
|----------------------------------|-------------------------------------------------------|
| `BuyEDalerService(Definition)`   | Creates a script-based service for buying eDaler(R).  |
| `PaymentService(Definition)`     | Creates a script-based service for payments.          |
| `SellEDalerService(Definition)`  | Creates a script-based service for selling eDaler(R). |

#### Serpro queries (available in TAG.Serpro.package)

The following functions are available on systems with the `TAG.Serpro.package` installed. A more detailed description
about the script functions referenced in this section is available in the [NeuronSerpro repository](https://github.com/Trust-Anchor-Group/NeuronSerpro).

| Function                        | Description                                                                                       |
|---------------------------------|---------------------------------------------------------------------------------------------------|
| `SerproCpfDfStatus()`           | Checks the status of the CPF-DF (Cadastro de Pessoas Físicas - Declaração de Fatos) API Service.  |
| `SerproCpfDf(CPF)`              | Makes a query against the CPF-DF (Cadastro de Pessoas Físicas - Declaração de Fatos) API Service. |
| `SerproDataValidStatus()`       | Checks the status of the DataValid (v4) API Service.                                              |
| `SerproDataValid(Claims,Photo)` | Makes a query against the DataValid (v4) API Service.                                             |

#### Sending SMS (available in TAG.Service.GatewayApi.package)

The following functions are available on systems with the `TAG.Service.GatewayApi.package` installed. A more detailed description
about the script functions referenced in this section is available in the [GatewayApiSms repository](https://github.com/Trust-Anchor-Group/GatewayApiSms).

| Function                                       | Description                                           |
|------------------------------------------------|-------------------------------------------------------|
| `SendGatewayApiSms(Sender,Message,Recipients)` | Sends an SMS (if service is configured correctly). `Recipients` can be a phone number or vector of phone numbers. |

#### OpenAI Extensions (available in TAG.XmppOpenAIBridge.package)

The following functions are available on systems with the `TAG.XmppOpenAIBridge.package` installed. A more detailed description
about the OpenAI-related functions referenced in this section is available in the [XmppOpenAIBridge repository](https://github.com/Trust-Anchor-Group/XmppOpenAIBridge).

| Function                                                                       | Description                                           |
|--------------------------------------------------------------------------------|-------------------------------------------------------|
| `ChatGpt(Instruction[,Sender],Text[,Functions],History[,Preview])`             | Calls the chat completion API of OpenAI (ChatGPT). The `Instruction` argument contains initialization instructions. The optional `Sender` argument contains the JID of the sender. If not provided, the JID of the quick-login user will be used. Text is the chat message to send. `Functions` contains a single function definition or a vector of function definitions the API can call if it chooses to. `History` is a boolean parameter that indicates if the session history should be included in the query. The optional `Preview` argument indicates if intermediate content responses are previewed during the execution of the query. The response to the call will be an object containing a `Content` property with textual content, a `Function` property with function call information if available, including a `Result` property, containing any results from a function call. If a function call is requested, available function definitions or lambda expressions will be checked. If available, they will be called, with the arguments available from the API. |
| `ChatGptConfigured()`                                                          | Checks if Chat GPT is configured correctly. It requires a Chat GPT<->XMPP Bridge node to be configured in the `MeteringTology` source, with the Node ID `ChatGPT`. |
| `ChatGptArray(Name,Description,Required,ItemParameter)`                        | Creates an array parameter for callback functions. The `ItemParameter` argument contains definition of each item in the array. |
| `ChatGptBoolean(Name,Description,Required)`                                    | Creates a Boolean parameter for callback functions. |
| `ChatGptEnum(Name,Description,Required,Values)`                                | Creates an enumeration parameter for callback functions. The `Values` argument contains a vector of strings representing the possible values the argument can take. |
| `ChatGptFunction(Name,Description,Parameters)`                                 | Creates a function definition for callback functions. The `Parameters` argument contains a vector of parameter definitions representing the arguments of the function. |
| `ChatGptInteger(Name,Description,Required[,MultipleOf])`                       | Creates an integer parameter for callback functions, possibly requiring it to be a multiple of a given base value. |
| `ChatGptInteger(Name,Description,Required[,Min,MinInc,Max,MaxInc])`            | Creates an integer parameter for callback functions, possibly within a specified range, between Min and Max, specifying also if the endpoints are included or not. |
| `ChatGptInteger(Name,Description,Required[,MultipleOf,Min,MinInc,Max,MaxInc])` | Creates an integer parameter for callback functions, possibly requiring it to be a multiple of a given base value, as well as within a specified range, between Min and Max, specifying also if the endpoints are included or not. |
| `ChatGptNumber(Name,Description,Required[,MultipleOf])`                        | Creates a number (float-point) parameter for callback functions, possibly requiring it to be a multiple of a given base value. |
| `ChatGptNumber(Name,Description,Required[,Min,MinInc,Max,MaxInc])`             | Creates a number (float-point) parameter for callback functions, possibly within a specified range, between Min and Max, specifying also if the endpoints are included or not. |
| `ChatGptNumber(Name,Description,Required[,MultipleOf,Min,MinInc,Max,MaxInc])`  | Creates a number (float-point) parameter for callback functions, possibly requiring it to be a multiple of a given base value, as well as within a specified range, between Min and Max, specifying also if the endpoints are included or not. |
| `ChatGptObject(Name,Description,Required,Properties)`                          | Creates an object parameter for callback functions. The `Properties` argument contains a vector of parameter definitions representing the properties of the object. |
| `ChatGptString(Name,Description,Required[,Pattern])`                           | Creates a string parameter for callback functions, having a regular expression to validate input. |
| `ChatGptString(Name,Description,Required[,Format])`                            | Creates a string parameter for callback functions, having a specific format, as given by the string format enumeration listed below. |

The String format enumeration can have the following values:

| Enumeration Value       | Corresponding JSON Schema string | Description or example |
|:------------------------|:---------------------------------|:-----------------------|
| `DateTime`              | `date-time`                      | Date and time together, for example, 2018-11-13T20:20:39+00:00. |
| `Time`                  | `time`                           | New in draft 7 Time, for example, 20:20:39+00:00 |
| `Date`                  | `date`                           | New in draft 7 Date, for example, 2018-11-13. |
| `Duration`              | `duration`                       | New in draft 2019-09 A duration as defined by the ISO 8601 ABNF for “duration”. For example, P3D expresses a duration of 3 days. |
| `EMail`                 | `email`                          | Internet email address, see RFC 5321, section 4.1.2. |
| `InternationalEMail`    | `idn-email`                      | New in draft 7 The internationalized form of an Internet email address, see RFC 6531. |
| `HostName`              | `hostname`                       | Internet host name, see RFC 1123, section 2.1. |
| `InternationalHostname` | `idn-hostname`                   | New in draft 7 An internationalized Internet host name, see RFC5890, section 2.3.2.3. |
| `IPv4`                  | `ipv4`                           | IPv4 address, according to dotted-quad ABNF syntax as defined in RFC 2673, section 3.2. |
| `IPv6`                  | `ipv6`                           | IPv6 address, as defined in RFC 2373, section 2.2. |
| `Uuid`                  | `uuid`                           | New in draft 2019-09 A Universally Unique Identifier as defined by RFC 4122. Example: 3e4666bf-d5e5-4aa7-b8ce-cefe41c7568a |
| `Uri`                   | `uri`                            | A universal resource identifier (URI), according to RFC3986. |
| `UriReference`          | `uri-reference`                  | New in draft 6 A URI Reference (either a URI or a relative-reference), according to RFC3986, section 4.1. |
| `Iri`                   | `iri`                            | New in draft 7 The internationalized equivalent of a “uri”, according to RFC3987. |
| `IriReference`          | `iri-reference`                  | New in draft 7 The internationalized equivalent of a “uri-reference”, according to RFC3987 |
| `UriTemplate`           | `uri-template`                   | New in draft 6 A URI Template (of any level) according to RFC6570. If you don’t already know what a URI Template is, you probably don’t need this value. |
| `JsonPointer`           | `json-pointer`                   | New in draft 6 A JSON Pointer, according to RFC6901. There is more discussion on the use of JSON Pointer within JSON Schema in Structuring a complex schema. Note that this should be used only when the entire string contains only JSON Pointer content, e.g. /foo/bar. JSON Pointer URI fragments, e.g. #/foo/bar/ should use "uri-reference". |
| `RelativeJsonPointer`   | `relative-json-pointer`          | New in draft 7 A relative JSON pointer. |
| `RegEx`                 | `regex`                          | New in draft 7 A regular expression, which should be valid according to the ECMA 262 dialect. |

Example:

```
ShowImage(Image):=
(
	Get(Image.Url) ??? "Image not available"
);

ShowImages(Images):=
(
	[foreach Image in Images do ShowImage(Image)]
);

R:=ChatGpt(
	"You help users find images on the Internet, representative of the queries made by the user.",
	"TestUser",
	"Can you find me some images of Kermit? If something is unclear, ask for additional information first. When ready to present images to the user, call available functions.",
	ChatGptFunction("ShowImages", "Displays an array of images to the user.", [
		ChatGptArray("Images", "Array of images to show.", true, 
			ChatGptObject("Image", "Information about an image.", true, [
				ChatGptString("Url", "URL to the image to show.", true),
				ChatGptInteger("Width","Width of image, in pixels.", false, 0, false, null, false),
				ChatGptInteger("Height","Height of image, in pixels.", false, 0, false, null, false),
				ChatGptString("Alt", "Alternative textual description of image, in cases the image cannot be shown.", false)]))]),
	false,
	true)
```

**Note**: If running script with ChatGPT-services on a web server, you can use the associated script functions to push information
asynchronously back to the web client using the `PushEvent` script function.

=========================================================================================================================================================

URI Schemes
------------------

URI Schemes recognized by the system depends on what modules are loaded. Classes with default constructors, implementing the
`Waher.Content.IContentGetter` interface will automatically be used when resources are requested using corresponding URI schemes.
The following table lists recognized URI schemes:

| URI Scheme | Module                        | Description |
|:-----------|:------------------------------|:------------|
| `aes256`   | `Waher.Networking.XMPP.HTTPX` | Retrieves content that has been previously encrypted. |
| `data`     | `Waher.Content`               | Resource retrieved using content embedded in the URI itself. |
| `http`     | `Waher.Content`               | Resource retrieved using the HTTP protocol. |
| `https`    | `Waher.Content`               | Resource retrieved using the HTTPS protocol (HTTP+SSL/TLS). |
| `httpx`    | `Waher.Networking.XMPP.HTTPX` | Resource retrieved using the [XEP-0332: HTTP over XMPP transport](https://xmpp.org/extensions/xep-0332.html) protocol. |
| `iotid`    | `Waher.Service.IoTBroker`     | Legal Identity resource. |
| `iotsc`    | `Waher.Service.IoTBroker`     | Smart Contract resource. |

=========================================================================================================================================================

Custom Parsers
--------------------------------------------

The script engine supports custom parses in external modules. Such parses can extend the script engine with new types of constructs in a way
function extensions cannot. The following subsections describe such extensions, and the libraries that publish them.

### Access to object database

The following extensions are made available by the `Waher.Script.Persistence` library.

#### SELECT

Simplified SQL `SELECT` statements can be executed against the object database. Matrices with named columns are returned. Calculated
columns are supported. Each record corresponds to an object, and variable references are by default interpreted as members of the current
object. To explicitly reference the object, you can use the `this` variable. You can also refer to the columns of the result set by using
the name of the corresponding column.

Syntax:

```
SELECT [TOP maxcount] [DISTINCT] [GENERIC]
	* |
	column1 [[as ]name1][, column2 [[as ]name2][, ...]]
FROM
	source1[ as sourcename1][, source2[ as sourcename2][, ...]]
(([INNER ]JOIN|LEFT[ OUTER] JOIN|RIGHT[ OUTER] JOIN|FULL[ OUTER] JOIN|OUTER JOIN)
	source[ as sourcename][ ON conditions])*
[WHERE
	conditions]
[GROUP BY
	group1 [groupname1][, group2 [groupname2][, ...]]
[HAVING
	groupconditions]]
[ORDER BY
	ordercolumn1[ ASC|DESC][, ordercolumn2[ ASC|DESC][, ...]]]
[OFFSET
	offset]
```

Example:

```
select
	Hour,
	count(Type) Nr,
	count(Type,EventType.Debug) Debug,
	count(Type,EventType.Informational) Informational,
	count(Type,EventType.Notice) Notice,
	count(Type,EventType.Warning) Warning,
	count(Type,EventType.Error) Error,
	count(Type,EventType.Critical) Critical,
	count(Type,EventType.Alert) Alert,
	count(Type,EventType.Emergency) Emergency
from
	PersistedEvent
where 
	Timestamp>=Now.AddDays(-1) 
group by 
	DateTime(Timestamp.Year,Timestamp.Month,Timestamp.Day,Timestamp.Hour,0,0) Hour
order by
	Hour
```

##### Implicit groups

By using aggregate functions in the column definitions, the parser assumes these to be calculated over the entire result set, 
if no group specification says otherwise. This implicit grouping is used, if any of the following functions are used:

* `Average`
* `Count`
* `Max`
* `Median`
* `Min`
* `Product`
* `StandardDeviation`
* `Sum`
* `Variance`

Example:

```
select
	min(Timestamp) "From",
	max(Timestamp) To
from
	PersistedEvent
```

##### Wildcards

You can also use wildcards (`*`) to represent any of the columns in a group. The wildcard also works in implicit groups.

Example:

```
select
	count(*) Nr
from
	PersistedEvent
where
	Type=EventType.Notice
```

##### Type Sources

The `SELECT` statement can search for information from different types of sources. These
sources are defined in the `FROM` clause. If they point to Type Names, they refer to
objects of the specified type that are stored in the object database.

##### Collection Sources

If the source returns a string value, or is a variable reference that does not point to a
type or variable, it is interpreted to represent the name of a collection in the object
database. Using such a source allows you to reference untyped objects in the corresponding
collection.

##### Vector Sources

The sources can also be script that returns any type of vector. In such a case, the `SELECT` 
statement operates directly on these vectors, without going to the database.

Example:

```
v:=[{a:1,b:2},{a:2,b:1}];
select a, b from v
```

##### Selecting a single object

If you select a single object, using a statement like `SELECT TOP 1 * ...` (here `TOP 1 *` identifies such a selection), either `null` 
or an object will be returned, instead of an empty array or an array of one or multiple objects.

##### SELECT FROM XML

The `SELECT` statement can also extract information from XML sources by using XPATH. Column extressions and `WHERE` clauses must be
made in XPATH, in order to operate on XML sources. XML Sources can be entire XML Documents, or XML Nodes inside an XML Document.
If the XPATH does not contain whitespace, and begins with a `/`, `.` or a `@`, it can be written as-is in the script. Otherwise, you can 
use the `XPath(expression)` function to convert a string value to an XPATH expression than can be used to select nodes from a XML source.
XPATH exrepssions used in column definition, extract values from the corresponding XML nodes selected, while XPATH expressions used in
`WHERE` clauses, refer to the nodes selected.

Example:

```
Xml:=<Books>
<Book price="10" title="Book 1"/>
<Book price="20" title="Book 2"/>
<Book price="30" title="Book 3"/>
</Books>;

select @title Title, @price Price from Xml where /Books/Book[@price>15];
```

#### INSERT VALUES

SQL `INSERT` statements can be executed against the object database. The inserted object is returned.

Syntax:

```
INSERT [LAZY]
INTO Source
(
	Column1[, 
	Column2[, 
	...[, 
	ColumnN]]]
)
VALUES
(
	Value1[, 
	Value2[, 
	...[, 
	ValueN]]]
)
```

Example:

```
insert
into PersistedEvent 
(
	Timestamp, 
	Message, 
	Object, 
	Actor
)
values
(
	Now, 
	"Kilroy was here", 
	"Here", 
	"Kilroy"
)
```

**Note**: Lazy inserts (or updates, deletions) are done when able. The statement may return before
the operation completes. The operation will be executed at the next opportune time.

#### INSERT SELECT

You can insert multiple objects into a data source by combining an `INSERT` and a `SELECT`
statement.

Syntax:

```
INSERT [LAZY]
INTO Source
SELECT ...
```

Example:

```
insert
into PersistedEvent 
select
	Timestamp,
	Message,
	Object,
	Actor
from
	...
```

#### INSERT OBJECT

You can insert an object ex nihilo into a collection using the `INSERT ... OBJECT` statement.

Syntax:

```
INSERT [LAZY]
INTO Source
OBJECT ...
```

Example:

```
insert
into PersistedEvent 
object
{
	Timestamp:Now, 
	Message:"Kilroy was here", 
	Object:"Here", 
	Actor:"Kilroy"
}
```

**Note**: When inserting an object, that object will be returned from the statement. This includes a reference to its newly generated
Object ID. If inserting an object ex-nihilo, the Object ID is found referring to the property `ObjectId` on the resulting object.

#### INSERT OBJECTS

You can insert a vector or set of objects ex nihilo into a collection using the 
`INSERT ... OBJECTS` statement.

Syntax:

```
INSERT [LAZY]
INTO Source
OBJECTS [Object1, ..., ObjectN]
```

Example:

```
insert
into PersistedEvent 
objects
[{
	Timestamp:Now, 
	Message:"Kilroy was here", 
	Object:"Here", 
	Actor:"Kilroy"
},
{
	Timestamp:Now, 
	Message:"Kilroy was here again", 
	Object:"Here", 
	Actor:"Kilroy"
}]
```

**Note**: Objects can be returned as a vector, set, or simply as a list of elements separated with commas.

**Note 2**: The objects returned will have their newly generated Object IDs. If inserting an objects ex-nihilo, the Object ID 
is found referring to the property `ObjectId` on the resulting objects.

#### UPDATE

Simplified SQL `UPDATE` statements can be executed against the object database. The number of objects updated is returned.

Syntax:

```
UPDATE [LAZY]
	source
SET
	property=value[,
	property2=value2[,...]]
[WHERE
	conditions]
```

Example:

```
update
	PersistedEvent
set
	Message="Kilroy "+Message
where 
	Type=EventType.Notice
```

#### DELETE

Simplified SQL `DELETE` statements can be executed against the object database. The number of objects deleted is returned.

Syntax:

```
DELETE [LAZY]
FROM
	source
[WHERE
	conditions]
```

Example:

```
delete
from
	PersistedEvent
where 
	Timestamp>=Now.AddDays(-1) and
	Type=EventType.Informational
```

#### CREATE

You can create indices in the object database using SQL `CREATE` statements.

Syntax:

```
CREATE INDEX
	Name
ON
	Source
(
	Field1[ ASC|DESC][,
	Field2[ ASC|DESC][,
	...]]
)
```

The Index Name can be used at a later stage, for instance, to drop the index.

Example:

```
CREATE INDEX
	WebUserEMail
ON
	WebUsers
(
	EMail,
	UserName
)
```

#### DROP

You can also drop indices and collections from the object database using SQL `DROP` statements.

##### DROP INDEX

To drop an index from the object database, use the `DROP INDEX` statement. Syntax:

```
DROP INDEX
	Name
ON
	Source
```

The Index Name is defined when creating indices using the `CREATE INDEX` command.

Example:

```
DROP INDEX
	WebUserEMail
ON
	WebUsers
```

##### DROP TABLE/COLLECTION

To drop a collection from the object database, use the `DROP COLLECTION` (or `DROP TABLE`) statement. Syntax:

```
DROP COLLECTION|TABLE
	Source
```

Example:

```
DROP COLLECTION
	WebUsers
```

### Access to Ledger

The following extensions are made available by the `Waher.Script.Persistence` library.

#### RECORD OBJECT

You can record an object ex nihilo into a collection in the Leger using the 
`RECORD ... OBJECT` statement.

Syntax:

```
RECORD
INTO Source
[NEW|UPDATE|DELETE] OBJECT ...
```

Example:

```
record
into PersistedEvent 
new object
{
	Timestamp:Now, 
	Message:"Kilroy was here", 
	Object:"Here", 
	Actor:"Kilroy"
}
```

**Note**: Object recorded directly to the ledger are not stored in the object database at
the same time. Object stored on the object database may be stored automatically in the
ledger, depending on the class definition and its corresponding archiving attributes.

#### RECORD OBJECTS

You can record a vector or set of objects ex nihilo into a collection in the Ledger using the 
`RECORD ... OBJECTS` statement.

Syntax:

```
RECORD
INTO Source
[NEW|UPDATE|DELETE] OBJECTS [Object1, ..., ObjectN]
```

Example:

```
record
into PersistedEvent 
new objects
[{
	Timestamp:Now, 
	Message:"Kilroy was here", 
	Object:"Here", 
	Actor:"Kilroy"
},
{
	Timestamp:Now, 
	Message:"Kilroy was here again", 
	Object:"Here", 
	Actor:"Kilroy"
}]
```

#### REPLAY

`REPLAY` statements can be executed against the ledger to extract entries matching certain
search criteria. By default, events matching the criteria will be output in the order they 
appear in the collections provided. You can direct the results to a given destination, which
can evaluate to a file name (in case the export with be an XML file) or an object instance 
implementing the `Waher.Persistence.Serialization.ILedgerExport` interface, or any of the
keywords `XML`, `JSON`, `COUNTERS` or `TABLE`, or evaluate to string values same as the
these keywords. If no `TO` clause is available, the default destination is `JSON` if no 
columns have been provided (i.e. `*` has been used), or `TABLE`, if columns have been 
provided. If an `ILedgerExport` interface or a file name is provided, the result of the 
execution (apart from exporting the replay to the destination) in the script environment 
will be the the same as providing a `COUNTERS` destination.

Syntax:

```
REPLAY [TOP maxcount]
	* |
	column1 [[as ]name1][, column2 [[as ]name2][, ...]]
FROM
	source1[ as sourcename1][, source2[ as sourcename2][, ...]]
[WHERE
	conditions]
[OFFSET
	offset]
[TO
	destination]
```

Example:

```
replay
	EventId,
	Level,
	Message
from
	PersistedEvent
where 
	Type="Error" 
to
	xml
```

**Note**: The Ledger does not have indices as the object database does. Replaying events
from the ledger often replay all encrypted blocks in entire collections, which may be time, 
memory and compute intensive operations.

##### Ledger event variables

When writing `REPLAY` conditions, you can refer to object properties using variable references.
There are also a set of predefined event propertyy names you can use to create conditions including
event property values. You can also access block and collection information. Block information
depends on the ledger registered. Some variables are available for all ledgers, others are
ledger-specific. If you need to check object properties with the same names as these event or
block property names, you can use `this` to refer to the object referenced by the event. For instance,
`Timestamp` would refer to the `Timestamp` property of the event, not a `Timestamp` property on
the associated object. Referring to `this.Timestamp` would access the `Timestamp` property on the
associated object.

Event properties available in `REPLAY` conditions:

| Event Property | Ledger       | Description                                                   |
|:---------------|:-------------|:--------------------------------------------------------------|
| `Collection`   | All          | Collection containg the block that contains the event.        |
| `BlockId`      | All          | Block ID containing the event.                                |
| `ObjectId`     | All          | Object ID of the associated object.                           |
| `TypeName`     | All          | Type name of the associated object.                           |
| `EntryType`    | All          | An enumeration of type `Waher.Persistence.EntryType`, that can take the values `New`, `Update` or `Delete`. (`Clear` is also a value, but not an option in conditional statements, as it is not used in association with objects. |
| `Timestamp`    | All          | The timestamp of the event.                                   |
| `this`         | All          | A reference to the recorded object associated with the event. |
| `Bytes`        | Neuro-Ledger | Number of bytes of the current block.                         |
| `Created`      | Neuro-Ledger | When the block was created.                                   |
| `Creator`      | Neuro-Ledger | The creator of the block.                                     |
| `Digest`       | Neuro-Ledger | The digest of the block.                                      |
| `Expires`      | Neuro-Ledger | When the block expires.                                       |
| `FileName`     | Neuro-Ledger | Local file name of the block.                                 |
| `Signature`    | Neuro-Ledger | Signature of the block.                                       |


### XML

The `Waher.Script.Xml` library extends the script engine to understand XML embedded in the script.
Attribute values are always considered to be script. You can provide constant strings, as usual,
but also provide script to dynamically populate your XML document with contents and calculations
based on current variable values. In element values, you can also embed script between the special 
`<[` and `]>` operators, or the corresponding `<(` and `)>` operators. Element names and attribute 
names are always interpreted literally. If you need to create dynamic XML, you can also build
a string, and use the `Xml()` function to convert it to an XML document.

#### XML Document

To create a simple XML document in script, simply enter:

	Doc:=<Root>Hello</Root>

You can add an XML declaration and processing instructions in the beginning of the document, if you
want:

	Doc:=<?xml version="1.0" encoding="UTF-8"?>
		<?xml-stylesheet type="text/xsl" href="style.xsl"?>
		<Root>Hello</Root>

You can also specify CDATA and comment sections in the code:

	Doc:=<a>
		<![CDATA[Hello World.]]>
		<!-- This is a comment -->
	</a>

#### Attribute values

You can add attributes to elements just as in normal XML:

	Doc:=<a>
		<b value="1"/>
		<b value="2"/>
	</a>

By default, attribute values are considered script, so you don't need to embed script using special
operators. Script priority for attribute values is the same as for powers. This means that if you
use operators with a lower priority than for powers, you need to use the parenthesis operators 
`(` and `)` to encapsulate the script expression. Example:

	x:=7;
	Doc:=<a>
		<b value=1/>
		<b value=(2+x)/>
	</a>

Script-based attributes are not output to the final XML, if the script returns `null`.
This makes it easy to define optional attributes. Example, here converting JSON with
optional fields to XML using pattern matching:

	Req:={
		"name":"Kalle",
		"age":50,
		"profession":"Bus Driver"
	};
	{
		"name":Str(Name),
		"age":0<=Int(Age)<=100,
		"profession":Str(Profession),
		"remarks":Optional(Str(Remarks))
	}:=Req;
	<Claim name=Name age=Age profession=Profession remarks=Remarks xmlns="https://example.org/Test"/>

The resulting XML would look like:

	<Claim name="Kalle" age="50" profession="Bus Driver" xmlns="https://example.org/Test"/>

#### Embedded expressions

To embed expressions in element text values, use the `<[` and `]>` operators, or the corresponding 
`<(` and `)>` operators:

	a:=2;
	b:=3;
	Doc:=<Root>
		<p>a=<[a]></p>
		<p>b=<[b]></p>
		<p>a+b=<[a+b]></p>
	</Root>

Script you embed can itself return XML. By using this feature, you can create dynamic XML content.
You can also return vectors, sets, or other enumerated types. By doing so, each item of the enumeration
will be added in sequence. In the following example, an array of small XML elements is embedded in a
root element.

	v:=1..10;
	<x><[[foreach x in v do <y value=x sqr=x^2/>]]></x>

The result would be as follows (new-lines and indentation added for readability):

```xml
<x>
	<y value="1" sqr="1" />
	<y value="2" sqr="4" />
	<y value="3" sqr="9" />
	<y value="4" sqr="16" />
	<y value="5" sqr="25" />
	<y value="6" sqr="36" />
	<y value="7" sqr="49" />
	<y value="8" sqr="64" />
	<y value="9" sqr="81" />
	<y value="10" sqr="100" />
</x>
```

Embedded XML and script can be nested to any level. The following example creates a simple multiplication table (kept short for brevity):

	x:=1..5;
	y:=1..5;
	<MultTable>
		<[[foreach a in x do <x value=a>
			<[[foreach b in y do <y value=b prod=(a*b)/>]]>
		</x>]]>
	</MultTable>

The result of this script is as follows. Here, whitespace is included in the script definition above, and is not added afterwards for readability.

```xml
<MultTable>
	<x value="1">
		<y value="1" prod="1" /><y value="2" prod="2" /><y value="3" prod="3" /><y value="4" prod="4" /><y value="5" prod="5" />
	</x><x value="2">
		<y value="1" prod="2" /><y value="2" prod="4" /><y value="3" prod="6" /><y value="4" prod="8" /><y value="5" prod="10" />
	</x><x value="3">
		<y value="1" prod="3" /><y value="2" prod="6" /><y value="3" prod="9" /><y value="4" prod="12" /><y value="5" prod="15" />
	</x><x value="4">
		<y value="1" prod="4" /><y value="2" prod="8" /><y value="3" prod="12" /><y value="4" prod="16" /><y value="5" prod="20" />
	</x><x value="5">
		<y value="1" prod="5" /><y value="2" prod="10" /><y value="3" prod="15" /><y value="4" prod="20" /><y value="5" prod="25" />
	</x>
</MultTable>
```

#### Wildcards in XML Pattern Matching

There are two types of wildcards you can use when performing pattern matching of XML content. Attribute wildcards are used to match any attribute
or attributes not listed in the pattern. An attribute wildcard is defined as `*`. Element wildcards are used to match any element or set of 
elements not listed in the pattern. An element wildcard is defined as `<*>`. Consider the following pattern matching script, that evaluates
successfully and extracts mentioned components:

```
<Person name=Required(Str(Name)) age=Optional(Int(Age))>
	<Profession><[Optional(Str(Profession))]></Profession>
	<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>
</Person>
:=
<Person name="Kalle" age="50">
	<Profession>Bus Driver</Profession>
	<EmployedSince>2010-01-02</EmployedSince>
</Person>;
```

But if you are only interested in the name and the date when the person was employed, you can use wildcards to ignore the other elements 
and attributes:

```
<Person name=Required(Str(Name)) *>
	<*>
	<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>
	<*>
</Person>
:=
<Person name="Kalle" age="50">
	<Profession>Bus Driver</Profession>
	<EmployedSince>2010-01-02</EmployedSince>
</Person>;
```

=========================================================================================================================================================

Interaction with .NET Code Behind classes
--------------------------------------------

Script can interact with .NET code running in the background. There are different methods to do this:

#. Referencing namespaces and types.
#. Calling static methods on types.
#. Creating objects.
#. Calling methods or accessing fields or properties on .NET objects.

### Referencing namespaces and types

Root namespaces recognized by the type inventory are available through normal variable references:

	System

Sub-namespaces are accessed through the member operator `.`:

	System.Collections.Generic

Types in namespaces are referenced through the member operator `.` on its namespace:

	System.Guid;
	System.Collections.Generic.List

Namespaces and types are values, and can be assigned to variables:

	S:=System;
	CG:=System.Collections.Generic;
	T:=CG.List;

Types and namespaces can be referenced using their *unqualified name* as long as no other type or namespace share the unqualified name.
If multiple types and/or namespaces share an unqualified name, referencing the unqualified name will return an array of qualified names
sharing the unqualified name.

### Calling static methods on types

Static methods on types are available as method calls using the member operator `.`. Parameters in method calls can be included as parameters in the normal fashion:

	ID1:=System.Guid.NewGuid();
	System.Text.Encoding.UTF8.GetBytes(ID1.ToString())

**Note**: If method is asynchronous, meaning it returns an object of type `Type<T>` for some type
`T`, the script engine will await for the task to complete, and return the finished result rather
than the `Task` object.

### Creating objects

You can create objects using the `Create` function. The first parameter contains the class of the object you want to create. The following arguments
contain any (optional) arguments you want to pass on to the constructor:

	Create(System.String,"*",10)

If the type is generic, type parameters must also be passed:

	L:=Create(System.Collections.Generic.List,System.String)

If a generic type requires parameters in the constructor, these are passed after the type arguments:

	Pair:=Create(System.Collections.Generic.KeyValuePair,System.String,System.Object,"A",3)

### Calling methods or accessing fields or properties on .NET objects

You can call methods and access fields and properties on an object, using the member operator `.`:

	L:=Create(System.Collections.Generic.List,System.String);
	L.Add("Hello");
	L.Add(" ");
	L.Add("World!");
	n:=L.Count;
	L.ToArray();

If an object has the index property `Item` defined, it can be accessed using the index operator:

	ID["FIRST"]

**Note**: If method is asynchronous, meaning it returns an object of type `Type<T>` for some type
`T`, the script engine will await for the task to complete, and return the finished result rather
than the `Task` object.

### Enumerated values

You can create enumerated value by simply referencing the type, and name, as you would in .NET: `TYPE.NAME`.
The `Type` would be a short name, fully qualified name, or a reference to a Type. The `Name` would be the
enumerated label. When calling .NET classes that accept numerical parameters, enumerated values are automatically
converted to such. The same is true if performing any of the logical/binary operators with enumerated values.
The results of the latter will be numerical values.

Example:

	FT:=Waher.Things.SensorData.FieldType;
	Types:=0;
							
	if Momentary then Types|=FT.Momentary;
	if Identity then Types|=FT.Identity;
	if Status then Types|=FT.Status;
	if Computed then Types|=FT.Computed;
	if Peak then Types|=FT.Peak;

### Utilizing operators defined in underlying objects.

If operators are defined in the underlying .NET code-behind, the script engine will utilize such
operators, for certain operators.

Examples, where a `DateTime` and a `TimeSpan` are operated upon:

	Now + TimeSpan(1,0,0)
	Now - TimeSpan(1,0,0)

Examples, where a `DateTime` and a `Duration` (defined in `Waher.Content` and 
`Waher.Script.Content`) are operated upon:

	Now + Duration("PT1H")
	Now - Duration("PT1H")

Code-behind operators are accessible as named static methods, in accordance with the following 
article (operator name corresponds to the *Metadata Name* column):
<https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/operator-overloads>

=========================================================================================================================================================

Physical Quantities
-----------------------

The script supports physical quantities, and can perform unit calculations and unit conversions. This simplifies many tasks such as comparing
sensor values or consolidating values from multiple sources using different units. To create a physical quantity, simply write the number
(or any expression), followed by the physical unit:

	10 m

You can use any SI prefix as well:

	10 km

You can use powers as well:

	10 m^2
	10 m²
	10 m³

You can multiply units, using the `⋅` or `*` operator:

	10 W⋅s
	10 W*s

Negative exponents are written either using negative exponents, or by using the `/` operator:

	10 m⋅s^-1
	10 m/s

The power operator `^` has higher precedence than `⋅`, `*` and `/`, so you can combine them safely:

	10 m^2/s
	10 m/s^2

Use parenthesis `(` and `)` to group units in the numerator or denominator:

	10 kg⋅m²/(A⋅s³)

Unit conversions are done implicitly in code when using addition, subtraction or comparison operators, as long as all values have 
corresponding units:

	10 m + 2 km
	2 km - 10 m
	10 m < 2 km
	10 °C > 20 °F
	10 m² > 1000 inch²
	10 V / 2 A = 5 Ohm

Unit arithmetic, including cancellation of terms, etc., is done when using multiplication or division operators:

	2 km * 10 m
	2 km² / 10 m
	10 m / 2 s

Explicit unit conversion can be performed by providing the unit to convert to behind an expression resulting in a physical quantity:

	10 km m = 10000 m
	10 kWh kJ = 36000 kJ

In the same way, it is possible to explicitly set the unit of an expression:

	10*sin(phi) m

If calling functions or operators normally accepting double values, the unit is stripped and the function or operator evoked with the magnitude
of the physical quantity only. Example:

	sin(10 W)

### Prefixes

All units can be prefixed by any of the following prefixes recognized by the script engine:

| Prefix | Name | Scale     |
|:------:|:-----|:---------:|
| Y      | Yotta | 10^[24]  |
| Z      | Zetta | 10^[21]  |
| E      | Eta   | 10^[18]  |
| P      | Peta  | 10^[15]  |
| T      | Tera  | 10^[12]  |
| G      | Giga  | 10^9     |
| M      | Mega  | 10^6     |
| k      | Kilo  | 10^3     |
| h      | Hecto | 10^2     |
| da     | Deka  | 10^1     |
| d      | Deci  | 10^[-1]  |
| c      | Centi | 10^[-2]  |
| m      | Milli | 10^[-3]  |
| µ, u   | Micro | 10^[-6]  |
| n      | Nano  | 10^[-9]  |
| p      | Pico  | 10^[-12] |
| f      | Femto | 10^[-15] |
| a      | Atto  | 10^[-18] |
| z      | Zepto | 10^[-21] |
| y      | Yocto | 10^[-24] |

**Note**: When there's an ambiguity of how to interpret a prefix with unit, and the system recognizes a unit with the full name, including the
prefix, the full unit will be chosen. Example: `ft` will be interpreted as *foot*, not *femto-tonnes*, `min` will be interpreted as *minute*,
not *milli-inches*, etc.

### Base Quantities

While any unit can be used to define a physical quantity, unit conversion not based in prefix changes can only be performed on units
recognized by the script engine. Such units are defined in *base quantities*, which are defined in code by creating classes with default
constructors implementing the `Waher.Script.Units.IBaseQuantity` interface. The following tables lists such base quantities as defined by
the `Waher.Script` library:

| Unit | Meaning  |
|:----:|:---------|
| m    | Metre    |
| Å    | Ångström |
| inch | Inch     |
| ft   | Feet     |
| foot | Feet     |
| yd   | Yard     |
| yard | Yard     |
| SM   | Statute Mile |
| NM   | Nautical Mile |
[Length]

**Note**: Since `IN` is a keyword, the unit *in* has to be written `inch`.

Furthermore, the following length units used in web applications are also recocnized:

| Unit | Meaning |
|:----:|:--------|
| px   | Pixels |
| pt   | Points |
| pc   | Picas  |
| em   | Relative to the font-size of the element |
| ex   | Relative to the x-height of the current font |
| ch   | Relative to the width of the "0" (zero) |
| rem  | Relative to font-size of the root element |
| vw   | Relative to 1% of the width of the viewport |
| vh   | Relative to 1% of the height of the viewport |
| vmin | Relative to 1% of viewport's smaller dimension |
| vmax | Relative to 1% of viewport's larger dimension |
[Length (Web)]

| Unit | Meaning |
|:----:|:--------|
| g    | Gram  |
| t    | Tonne |
| u    | Atomic mass unit |
| lb   | Pound |
[Mass]

| Unit | Meaning |
|:----:|:--------|
| s    | Second  |
| min  | Minute  |
| h    | Hour    |
| d    | Day     |
| w    | Week    |
[Time]

| Unit | Meaning |
|:----:|:--------|
| A    | Ampere  |
[Current]

| Unit | Meaning    |
|:----:|:-----------|
| °C   | Celcius    |
| °F   | Farenheit  |
| K    | Kelvin     |
[Temperature]

A special kind of base unit is also available for dimensionless units:

| Unit  | Meaning     |
|:-----:|:------------|
| 1     | One         |
| pcs   | Pieces      |
| gr    | Gross       |
| gross | Gross       |
| dz    | Dozen       |
| dozen | Dozen       |
| rad   | Radians     |
| deg   | Degrees     |
| °     | Degrees     |
| %     | Percent     |
| ‰     | Permille    |
| %0    | Permille    |
| ‱    | Perdixmille |
| %00   | Perdixmille |
[Dimensionless]

### Derived Quantities

Apart from the base quantities defined above, and their combinations, exponents and factors, the script engine also handles *derived quantities*, 
which are defined in code by creating classes with default constructors implementing the `Waher.Script.Units.IDerivedQuantity` interface. 
The following tables lists such derived quantities as defined by the `Waher.Script` library:

| Unit | Meaning |
|:----:|:--------|
| F    | 1 s^4⋅A²/(m²⋅kg) |
[Capacitance]

| Unit | Meaning |
|:----:|:--------|
| C    | 1 s⋅A |
[Electric Charge]

| Unit | Meaning |
|:----:|:--------|
| J    | 1 kg⋅m²/s² |
| BTU  | 1055 Mg⋅m²/s² |
[Energy]

| Unit | Meaning |
|:----:|:--------|
| N    | 1 kg⋅m/s² |
[Force]

| Unit | Meaning |
|:----:|:--------|
| Hz   | 1 s^-1 |
| cps  | 1 s^-1 |
| rpm  | 1 min^-1 |
[Frequency]

| Unit | Meaning |
|:----:|:--------|
| W    | 1 kg⋅m²/s³ |
[Power]

| Unit | Meaning |
|:----:|:--------|
| Pa   | 1 kg/(m⋅s²) |
| bar  | 100 Mg/(m⋅s²) |
| psi  | 6894.757 kg/(m⋅s²) |
| atm   | 101352.9279 kg/(m⋅s²) |
[Pressure]

| Unit | Meaning |
|:----:|:--------|
| Ω, Ohm, ohm    | 1 m²·kg/(s³·A²) |
[Resistance]

| Unit | Meaning |
|:----:|:--------|
| knot  | 0.514444 m/s |
| kn    | 0.514444 m/s |
| kt    | 0.514444 m/s |
[Speed]

| Unit | Meaning |
|:----:|:--------|
| V    | 1 kg⋅m²/(A⋅s³) |
[Voltage]

| Unit | Meaning |
|:----:|:--------|
| l  | 0.001 m³ |
[Volume]

### Compound Units

Compound units are units that are written as a string, but in actuality is a sequence of unit factors. The following tables lists compound
units recognized by the `Waher.Script` library:

| Unit | Meaning |
|:----:|:--------|
| Wh   | W⋅h |
[Energy]

| Unit | Meaning |
|:----:|:--------|
| mph   | SM/h |
| fps   | ft/s |
[Speed]

=========================================================================================================================================================

Measurements
-----------------

The script also supports measurements, which contain a magnitude and an error. The magnitude can
be a number or a physical quantity. Artithmetics operators propagate errors accordingly.
The `±` character is used to separate the magnitude from the error. For ease of typing,
`+-` can also be used to the same effect.

Examples:

	10 m +- 1cm
	(10 m +- 1cm) + (2 km +- 10m)
	(10 m +- 1cm) * (2 s +- 100ms)
	(10 m +- 1cm) / (2 s +- 100ms)

If `m` is a measurement, you can use `m.Estimate` to access the estimated or expected
value, and `m.Max` and `m.Min` to get the maximum and minimum values in the range
respectively. Values will be returned as Physical Quantities. Note that necessary unit 
conversions will be performed.
