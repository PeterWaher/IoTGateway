Title: Script
Description: Script syntax reference, as understood by the IoT Gateway.
Date: 2016-02-26
Author: Peter Waher
Copyright: Copyright.md
Master: Master.md

Script syntax reference
=============================

The **IoT Gateway** contains a powerful script parser and evaluation engine. The script engine is not ECMA-compliant. Instead, its focus
is to provide an efficient and compact script language using mathemathical notation. Following is a brief overview of the syntax different 
script elements. You can also use the [Calculator](Calculator.md) to experiment with script syntax.

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
|:----------------------:|----------------------------------------------------------|
| `e`                    | Euler's number                                            |
| `π`, `pi`              | Pi                                                        |
| `ε`, `eps`, `epsilon`  | Smallest positive double value that is greater than zero. |
| `∞`, `inf`, `infinity` | Positive infinity.                                        |
| `i`                    | Imaginary unit                                            |
| `C`                    | Set of complex numbers                                    |
| `R`                    | Set of real numbers                                       |
| `∅`, `EmptySet`        | The empty set                                             |
| `Now`                  | Current date and time                                     |
| `Today`                | Current date                                              |

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

Vectors can be explicitly created by listing their elements between square brackets `[` and `]`, or implicitly by inserting a `DO`-`WHILE`, `WHILE`-`DO`,
`FOR`-`TO`[-`STEP`][-`DO`] or `FOR EACH`/`FOREACH`-`IN`[-`DO`] statements between square brackets. Examples:

	v:=[1,2,3];
	v:=[DO x++ WHILE X<10];
	v:=[WHILE x<10 : x++];
	v:=[FOR x:=1 TO 20 STEP 3 : x];
	v:=[FOREACH x IN 1..10|0.1 : x^2];
	v:=[FOR EACH x IN 1..10|0.1 : x^2];

**Note*: `DO` can be exchanged with `:`, or completely omitted, except in the `DO`-`WHILE` case.

#### Matrices

Matrices can be explicitly created by listing their row vectors between square brackets `[` and `]`, or implicitly by inserting a `DO`-`WHILE`, `WHILE`-`DO`,
`FOR`-`TO`[-`STEP`][-`DO`] or `FOR EACH`/`FOREACH`-`IN`[-`DO`] statements between square brackets. Examples:

	M:=[[1,0,0],[0,1,0],[0,0,1]];
	M:=[DO [x++,x++,x++] WHILE X<10];
	M:=[WHILE x<10 : [x++,x++,x++]];
	M:=[FOR y:=1 TO 20 : [FOR x:=1 TO 20 : x=y ? 1 : 0]];
	M:=[FOREACH x IN 1..10|0.1 : [x^2,x^3,x^4]];
	M:=[FOR EACH x IN 1..10|0.1 : [x^2,x^3,x^4]];

**Note*: `DO` can be exchanged with `:`, or completely omitted, except in the `DO`-`WHILE` case.

#### Sets

Sets can be explicitly created by listing their elements between braces `{` and `}`, or implicitly by inserting a `DO`-`WHILE`, `WHILE`-`DO`,
`FOR`-`TO`[-`STEP`][-`DO`] or `FOR EACH`/`FOREACH`-`IN`[-`DO`] statements between braces. Examples:

	S:={1,2,3};
	S:={DO x++ WHILE X<10};
	S:={WHILE x<10 : x++};
	S:={FOR x:=1 TO 20 STEP 3 : x};
	S:={FOREACH x IN 1..10|0.1 : x^2};
	S:={FOR EACH x IN 1..10|0.1 : x^2};

**Note*: `DO` can be exchanged with `:`, or completely omitted, except in the `DO`-`WHILE` case.

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

### Suffix-operators

Suffix-operators are written after the operand to which they are applied. The following table lists available suffix operators:

| Operator      | Description                                 | Example      |
|:-------------:|:--------------------------------------------|:------------:|
| `.`           | Member operator                             | `obj.Member` |
| `(` List `)`  | Function evaluation                         | `f(a,b,c)`   |
| `[]`          | To vector, if not already                   | `a[]`        |
| `[Index]`     | Vector index operator                       | `v[i]`       |
| `[X,Y]`       | Matrix index operator                       | `M[x,y]`     |
| `[X,]`        | Matrix colum vector operator                | `M[x,]`      |
| `[,Y]`        | Matrix row vector operator                  | `M[,y]`      |
| `[,]`         | To matrix, if not already                   | `a[,]`       |
| `{}`          | To set, if not already                      | `a{}`        |
| `++`          | Post-Increment                              | `a++`       |
| `--`          | Post-Decrement                              | `a--`       |
| `%`           | Percent                                     | `10%`       |
| `‰`           | Per thousand                                | `20‰`       |
| `‱`         | Per ten thousand                           | `30‱`     |
| `°`           | Degrees to radians                          | `sin(100°)` |
| `'`           | Default differentiation (prim)              | `f'(x)`     |
| `′`           | Default differentiation (prim)              | `f′(x)`     |
| `"`           | Default second-order differentiation (bis)  | `f"(x)`     |
| `″`           | Default second-order differentiation (bis)  | `f″(x)`     |
| `‴`           | Default third-order differentiation         | `f‴(x)`     |
| `T`           | Transposed matrix                           | `M T`       |
| `H`           | Conjugate Transposed matrix                 | `M H`       |
| `†`           | Conjugate Transposed matrix                 | `M†`        |
| `!`           | Faculty                                     | `n!`        |
| `!!`          | Semi-Faculty                                | `n!!`       |
| Physical unit | Defines a physical quantity.                | `10 m/s`    |

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

| Operator   | Description                       | Example            |
|:----------:|:----------------------------------|:------------------:|
| `<=`       | Lesser than or equal to           | `a <= b`           |
| `<`        | Lesser than                       | `a < b`            |
| `>=`       | Greater than or equal to          | `a >= b`           |
| `>`        | Greater than                      | `a > b`            |
| `=`        | Equal to                          | `a = b`            |
| `==`       | Equal to                          | `a == b`           |
| `===`      | Identical to                      | `a === b`          |
| `<>`       | Not Equal to                      | `a <> b`           |
| `!=`       | Not Equal to                      | `a != b`           |
| `LIKE`     | Matches regular expression        | `a LIKE regex`     |
| `NOT LIKE` | Does not match regular expression | `a NOT LIKE regex` |
| `NOTLIKE`  | Does not match regular expression | `a NOTLIKE regex`  |
| `UNLIKE`   | Does not match regular expression | `a UNLIKE regex`   |
| `.=`       | Equal to (element-wise)           | `a .= b`           |
| `.==`      | Equal to (element-wise)           | `a .== b`          |
| `.===`     | Identical to (element-wise)       | `a .=== b`         |
| `.<>`      | Not Equal to (element-wise)       | `a .<> b`          |
| `.!=`      | Not Equal to (element-wise)       | `a .!= b`          |

**Note**: Element-wise variant of operators only exist for equality or non-equality operators, since these are also defined when comparing encapsulating
objects such as sets, vectors, arrays, matrices, etc.

**Note 2**: If regular expressions contain named groups, variables with the corresponding names will be set to the contents of the corresponding
groups if the regular expression matches the string.

### Membership operators

There are various different membership operators. All have the same [order of presedence][].

| Operator   | Description                       | Example            |
|:----------:|:----------------------------------|:------------------:|
| `AS`       | The `AS` operator makes sure the left operand is of the same type as the right operand. The result is **null** if they are not, or the same value as the left operand if they are.           | `Value as Type`           |
| `IS`       | The `IS` operator checks if the left operand is of the same type as the right operand. | `Value is Type` |
| `IN`       | The `IN` operator checks if the left operand is a member of the right operand. | `Value in Set` |
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

If the left side is not a variable reference, a pattern matching algorithm is employed that tries to assign all implicitly available variable references
by comparing both sides. Example:

	[x,y]:=f(a,b,c)

In the above example, the function `f`, which takes three parameters, is supposed to return a vector of two elements. If it does, the variables `x` and
`y` are assigned the elements of this return vector.

There's also a set of aritmethic operators that act directly on a variable value. These are also categorized as assignment operators, and have the same
[order of presedence][]. These operators are:

| Operator                   | Meaning                   | Example                                   |
|:--------------------------:|:-------------------------|:-----------------------------------------:|
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

Conditional `IF`-statements can be written in various ways. Either using the `IF` and `THEN` keywords, followed by the optional `ELSE` keyword, or by
using the `?` operator, followed by the optional `:` operator. Examples:

	IF Condition THEN IfTrueStatement
	IF Condition THEN IfTrueStatement ELSE IfFalseStatement
	Condition ? IfTrueStatement
	Condition ? IfTrueStatement : IfFalseStatement

**Note**: `IF`, `THEN` and `ELSE` are case insensitive. They are written here using upper case for clarity.

**Note 2**: If no `ELSE` or `:` is present, the statement is evaluated to **null**.

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

### Conditional Statements (DO/WHILE, WHILE/DO, FOR, FOREACH, TRY CATCH FINALLY)

There are multiple ways to execute conditional loops. These statements have the same [order of presedence][]:

| Operator | Meaning                   | Example              |
|:--------:|:--------------------------|:--------------------:|
| `DO` ... `WHILE` ... | Performs an action until a condition becomes **true**. | `DO Statement WHILE Condition` |
| `WHILE` ... [`DO`] ... | While a condition is **true**, performs an action. | `WHILE Condition DO Statement` |
| `FOREACH` ... `IN` ... [`DO`] ... | Iterates a variable through an enumerable set of values and performs an action on each iterated value. | `FOREACH Variable in Collection DO Statement` |
| `FOR EACH` ... `IN` ... [`DO`] ... | Iterates a variable through an enumerable set of values and performs an action on each iterated value. | `FOR EACH Variable in Collection DO Statement` |
| `FOR` ... `:=` ... `TO` ... [`STEP` ...] [`DO`] ... | Iterates a variable through a sequence of numerical values and performs an action on each iterated value. | `FOR Variable:=From TO Stop STEP StepSize DO Statement` |
| `TRY` ... `CATCH` ... `FINALLY` ... | Executes a statement. If an exception occurs, it is caught and an exception statement is executed. Afterwards, a finalization statement is executed. The exception object will be available in the CATCH statement, under the name of `Exception`. | `FOR Statement CATCH Exception FINALLY Done` |
| `TRY` ... `CATCH` ... | Executes a statement. If an exception occurs, it is caught and an exception statement is executed. The exception object will be available in the CATCH statement, under the name of `Exception`. | `FOR Statement CATCH Exception` |
| `TRY` ... `FINALLY` ... | Executes a statement. Afterwards, a finalization statement is executed regardless if an exception has been thrown or not. Any exceptions are automatically propagated. | `FOR Statement FINALLY Done` |
| `]]`...`[[` | Implicit print statement. This operation prints the contents between the `]]` and `[[` to the current console output. Any expressions embedded between `((` and `))` will be evaluated and the result displayed. | `a:=10;]]Value of a: ((a)).[[;` |

**Note**: The `DO` keyword can be replaced by a `:`. 

**Note 2**: The use of the `STEP` keyword together with the step size is optional. If omitted, a default step size of `1` or `-1` will be used, depending
if the loop is ascending or descending.

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

| Function | Description | Example |
|----------|-------------|---------|
| `Exp(z)` | `e` raised to the power of `z`. | `Exp(10)` |
| `Ln(z)` | Natural logarithm of `z`. | `Ln(e)` |
| `Lg(z)` | Base-10 logarithm of `z`. | `Lg(10)` |
| `Log10(z)` | Same as `lg`. | `Lg(10)` |
| `Log2(z)` | Base-2 logarithm of `z`. | `Log2(2)` |
| `Sqrt(z)` | Square root of `z`. | `Sqrt(2)` |

#### Trigonometric functions

The following table lists available trigonometric functions:

| Function | Description | Example |
|----------|-------------|---------|
| `Cos(z)` | Cosine, `z` in radians. | `Cos(100°)` |
| `Cot(z)` | Cotangent, `z` in radians. | `Cot(100°)` |
| `Csc(z)` | Cosecant, `z` in radians. | `Csc(100°)` |
| `Sec(z)` | Secant, `z` in radians. | `Sec(100°)` |
| `Sin(z)` | Sine, `z` in radians. | `Sin(100°)` |
| `Tan(z)` | Tangent, `z` in radians. | `Tan(100°)` |
| `ACos(z)` | Same as `ArcCos(z)`. | `ACos(Cos(100°))` |
| `ACot(z)` | Same as `ArcCot(z)`. | `ACot(Cot(100°))` |
| `ACsc(z)` | Same as `ArcCsc(z)`. | `ACsc(Csc(100°))` |
| `ASec(z)` | Same as `ArcSec(z)`. | `ASec(Sec(100°))` |
| `ASin(z)` | Same as `ArcSin(z)`. | `ASin(Sin(100°))` |
| `ATan(z)` | Same as `ArcTan(z)`. | `ATan(Tan(100°))` |
| `ATan(x,y)` | Same as `ArcTan(x,y)`. | `ATan(3,4)` |
| `ArcCos(z))` | Inverse Cosine. | `ArcCos(Cos(100°))` |
| `ArcCot(z))` | Inverse Cotangent. | `ArcCot(Cot(100°))` |
| `ArcCsc(z))` | Inverse Cosecant. | `ArcCsc(Csc(100°))` |
| `ArcSec(z))` | Inverse Secant. | `ArcSec(Sec(100°))` |
| `ArcSin(z)` | Inverse Sine. | `ArcSin(Sin(100°))` |
| `ArcTan(z))` | Inverse Tangent. | `ArcTan(Tan(100°))` |
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
| `ACosH(z)` | Same as `ArcCosH(z)`. | `ACosH(CosH(100°))` |
| `ACotH(z)` | Same as `ArcCotH(z)`. | `ACotH(CotH(100°))` |
| `ACscH(z)` | Same as `ArcCscH(z)`. | `ACscH(CscH(100°))` |
| `ASecH(z)` | Same as `ArcSecH(z)`. | `ASecH(SecH(100°))` |
| `ASinH(z)` | Same as `ArcSinH(z)`. | `ASinH(SinH(100°))` |
| `ATanH(z)` | Same as `ArcTanH(z)`. | `ATanH(TanH(100°))` |
| `ArcCosH(z))` | Inverse Hyperbolic Cosine. | `ArcCosH(CosH(100°))` |
| `ArcCotH(z))` | Inverse Hyperbolic Cotangent. | `ArcCotH(CotH(100°))` |
| `ArcCscH(z))` | Inverse Hyperbolic Cosecant. | `ArcCscH(CscH(100°))` |
| `ArcSecH(z))` | Inverse Hyperbolic Secant. | `ArcSecH(SecH(100°))` |
| `ArcSinH(z)` | Inverse Hyperbolic Sine. | `ArcSinH(SinH(100°))` |
| `ArcTanH(z))` | Inverse Hyperbolic Tangent. | `ArcTanH(TanH(100°))` |

### Scalar Functions

The following table lists available scalar functions:

| Function | Description | Example |
|----------|-------------|---------|
| `Abs(z)` | Absolute value (or magnitude of) `z` | `Abs(-1)` |
| `Ceiling(z)` | Round `z` up to closest integer. | `Ceiling(pi)` |
| `Ceil(z)` | Same as `Ceiling(z)`. | `Ceil(-1)` |
| `Floor(z)` | Round `z` down to closest integer. | `Floor(pi)` |
| `Max(x,y)` | Largest of `x` and `y`. | `Max(10,a)` |
| `Min(x,y)` | Smallest of `x` and `y`. | `Min(10,a)` |
| `Num(x)` | Same as `Number(x)`. | `Num('100')` |
| `Number(x)` | Converts `x` to a number. | `Number('100')` |
| `Round(z)` | Round `z` up or down to closest integer. | `Round(pi)` |
| `Sign(z)` | Sign of `z` (-1/0/1 + -i/0/+i). | `Sign(pi)` |
| `Str(x)` | Same as `String(x)`. | `Str(100)` |
| `String(x)` | Converts `x` to a string. | String(100)` |

### Complex Functions

The following table lists available scalar functions:

| Function | Description | Example |
|----------|-------------|---------|
| `Arg(z)` | Argument (or phase) of `z`. | `Arg(2+i)` |
| `Conj(z)` | Same as `Conjugate(z)`. | `Conj(2+i)` |
| `Conjugate(z)` | Conjugate of `z`. | `Conjugate(2+i)` |
| `Im(z)` | Imaginary part of `z`. | `Im(2+i)` |
| `Polar(n,φ)` | Complex number given in polar coordinates `n` and `φ`. | `Polar(1,pi/2)` |
| `Re(z)` | Real part of `z`. | `Re(2+i)` |

### String Functions

The following table lists available string-related functions:

| Function | Description | Example |
|----------|-------------|---------|
| `Empty(s)` | Same as `IsEmpty(s)`. | `Empty(s)` |
| `Eval(s)` | Same as `Evaluate(s)`. | `Evaluate("a+b")` |
| `Evaluate(s)` | Parses the string and evaluates it. | `Evaluate("a+b")` |
| `IsEmpty(s)` | Returns a boolean value showing if the string `s` is empty or not. | `IsEmpty(s)` |
| `Left(s,N)` | Returns a string with the left-most `N` characters. If the string `s` is shorter, the entire string is returned. | `Left(s,3)` |
| `Len(s)` | Same as `Length(s)`. | `Len(s)` |
| `Length(s)` | Returns the length of the string. | `Length(s)` |
| `Mid(s,Pos,Len)` | Returns a substring of `s`, starting a character `Pos` and continuing `Len` characters. The `Pos` index is zero-based. If the requested substring goes beyond the scope of `s`, the substring gets truncated accordingly. | `Mid(s,5,2)` |
| `Parse(s)` | Parses the string as an expression, and returns the parsed expression. | `Parse("a+b")` |
| `Right(s,N)` | Returns a string with the right-most `N` characters. If the string `s` is shorter, the entire string is returned. | `Right(s,3)` |

### Date and Time Functions

| Function | Description | Example |
|----------|-------------|---------|
| `DateTime(Year,Month,Day)` | Creates a Date value. | `DateTime(2016,03,05)` |
| `DateTime(Year,Month,Day,Hour,Minute,Second)` | Creates a Date and Time value. | `DateTime(2016,03,05,19,17,23)` |
| `DateTime(Year,Month,Day,Hour,Minute,Second,MSecond)` | Creates a Date and Time value. | `DateTime(2016,03,05,19,17,23,123)` |

### Vector Functions

The following functions operate on vectors:

| Function | Description | Example |
|----------|-------------|---------|
| `And(v)` | Logical or binary AND of all elements in vector | `And([1,2,3,4,5])`, `And([true,false,true])` |
| `Avg(v)` | Same as `Average(v)` | `Avg([1,2,3,4,5])` |
| `Average(v)` | Average of elements in the vector `v`. | `Average([1,2,3,4,5])` |
| `Join(v1,v2[,v3[,v4[,v5[,v6[,v7[,v8[,v9]]]]]]])` | Joins a sequence of vectors, into a larger vector. | `Join(v1,v2)` |
| `Max(v)` | The largest element in the vector `v`. | `Max([1,2,3,4,5])` |
| `Median(v)` | The median element in the vector `v`. | `Median([1,2,3,4,5])` |
| `Min(v)` | The smallest element in the vector `v`. | `Min([1,2,3,4,5])` |
| `Nand(v)` | Logical or binary NAND of all elements in vector | `Nand([1,2,3,4,5])`, `Nand([true,false,true])` |
| `Nor(v)` | Logical or binary NOR of all elements in vector | `Nor([1,2,3,4,5])`, `Nor([true,false,true])` |
| `Ones(N)` | Creates an N-dimensional vector with all elements set to 1. | `Ones(5)` |
| `Or(v)` | Logical or binary OR of all elements in vector | `Or([1,2,3,4,5])`, `Or([true,false,true])` |
| `Prod(v)` | Same as `Product(v)` | `Prod([1,2,3,4,5])` |
| `Product(v)` | Product of elements in the vector `v`. | `Product([1,2,3,4,5])` |
| `Reverse(v)` | Returns a vector with the elements of the original vector `v` in reverse order. | `Reverse([1,2,3,4,5])` |
| `StdDev(v)` | Same as `StandardDeviation(v)` | `StdDev([1,2,3,4,5])` |
| `StandardDeviation(v)` | Standard deviation of elements in the vector `v`. | `StandardDeviation([1,2,3,4,5])` |
| `Sum(v)` | Sum of elements in the vector `v`. | `Sum([1,2,3,4,5])` |
| `Var(v)` | Same as `Variance(v)` | `Var([1,2,3,4,5])` |
| `Variance(v)` | Variance of elements in the vector `v`. | `Variance([1,2,3,4,5])` |
| `Xnor(v)` | Logical or binary XNOR of all elements in vector | `Xnor([1,2,3,4,5])`, `Xnor([true,false,true])` |
| `Xor(v)` | Logical or binary XOR of all elements in vector | `Xor([1,2,3,4,5])`, `Xor([true,false,true])` |
| `Zeroes(N)` | Creates an N-dimensional vector with all elements set to 0. | `Zeroes(5)` |

### Matrix Functions

The following functions operate on matrices:

| Function | Description | Example |
|----------|-------------|---------|
| `Identity(N)` | Creates an NxN identity matrix. | `Identity(10)` |
| `Inv(M)` | Same as `Invert(M)`. | `Inv([[1,1],[0,1]])` |
| `Inverse(M)` | Same as `Invert(M)`. | `Inverse([[1,1],[0,1]])` |
| `Invert(M)` | Inverts `M`. Works on any invertable element. | `Invert([[1,1],[0,1]])` |

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

| Function | Description | Example |
|----------|-------------|---------|
| `Create(Type[,ArgList])` | Creates an object instance of type `Type`. `ArgList` contains an optional list of arguments. If `Type` is a generic type, the generic type arguments precede any constructor arguments. | `Create(System.String,'-',80)` |
| `Delete(x)` | Same as `Destroy(x)`. | `Delete(x)` |
| `Destroy(x)` | Destroys the value `x`. If the function references a variable, the variable is also removed. | `Destroy(x)` |
| `Error(Msg)` | Throws an error/exception. | `Error('Something went wrong.')` |
| `Exception(Msg)` | Same as `Error(Msg)`. | `Exception('Something went wrong.')` |
| `Exists(f)` | Checks if the expression defined by `f` is valid or not. | `Exists(x)` |
| `Print(Msg)` | Prints a message to the current console output (which is defined in the variables collection). | `Print(x)` |
| `PrintLine(Msg)` | Prints a message followed by a newline to the current console output. | `PrintLine(x)` |
| `PrintLn(Msg)` | Same as `PrintLine(Msg)`. | `PrintLine(x)` |
| `Remove(Var)` | Removes the varable `Var` without destroying its contents. | `Remove(x)` |
| `Return(x)` | Returns from the current function scope with the value `x`. | `return(Result)` |

### Extensions

The script engine can be extended by modules that are run in the environment. The following subssections list such funcion extensions
made available in different modules available by default on the gateway. This list does not include funcion extensions made available
by applications that are not part of the **IoT Gateway**.

#### Color functions

The following functions are available in the `Waher.Script.Graphs` library.

| Function | Description | Example |
|----------|-------------|---------|
| `GrayScale(Color)` | Converts a color to its corresponding Gray-scale value. | `GrayScale(cl)` |
| `HSL(H,S,L)` | Creates a color from its HSL representation. | `HSL(100,0.5,0.7)` |
| `HSLA(H,S,L,A)` | Creates a color from its HSLA representation. | `HSLA(100,0.5,0.7,64)` |
| `HSV(H,S,V)` | Creates a color from its HSV representation. | `HSV(100,0.5,0.7)` |
| `HSVA(H,S,V,A)` | Creates a color from its HSVA representation. | `HSVA(100,0.5,0.7,64)` |
| `RGB(R,G,B)` | Creates a color from its RGB representation. | `RGB(100,150,200)` |
| `RGBA(R,G,B,A)` | Creates a color from its RGBA representation. | `RGBA(100,150,200,64)` |

#### Graph functions (Waher.Script.Graphs)

The following functions are available in the `Waher.Script.Graphs` library.

| Function | Description | Example |
|----------|-------------|---------|
| `Plot2DCurve(X,Y[,Color[,PenSize[,Tension]]])` | Plots a smooth two-dimensional curve. | [Example][Plot2DCurveExample] |
| `Plot2DLine(X,Y[,Color[,PenSize]])` | Plots a segmented two-dimensional line. | [Example][Plot2DLineExample] |

[Plot2DCurveExample]: Calculator.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0aplot2dcurve(x,y)
[Plot2DLineExample]: Calculator.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0aplot2dline(x,y)

The following table lists variables that control graph output:

| Varaible | Description | Defaut value |
|----------|-------------|-------------|
| GraphWidth | Width of graph, in pixels. | 640 |
| GraphHeight | Height of graph, in pixels. | 480 |

#### Persistence-related functions

The following functions are available in the `Waher.Script.Persistence` library.

| Function | Description | Example |
|----------|-------------|---------|
| `DeleteObject(Obj)` | Deletes an object from the underlying persistence layer. | `Delete(Obj)` |
| `FindObjects(Type, Offset, MaxCount, Filter, SortOrder)` | Finds objects of a given `Type`. `Offset` and `MaxCount` provide a means to paginate the result set. `Filter` can be null, if none is used, or a string containing an expression to limit the result set. `SortOrder` sorts the result. It also determines the index to use. | `FindObjects(Namespace.CustomType, 0, 10, "StringProperty='StringValue'", ["Property1","Property2"])` |
| `SaveNewObject(Obj)` | Saves a new object to the underlying persistence layer. | `SaveNewObject(Obj)` |
| `UpdateObject(Obj)` | Updaes an object in the underlying persistence layer. | `UpdateObject(Obj)` |

#### Content-related functions (Waher.Content)

The following functions are available in the `Waher.Content` library.

| Function | Description | Example |
|----------|-------------|---------|
| `HtmlAttributeEncode(s)` | Encodes a string for inclusion in an HTML attribute. It transforms `<`, `>`, `&` and `"` to `&lt;`, `&gt;`, `&amp;` and `&quot;` correspondingly. | [Example][HtmlAttributeEncodeExample] |
| `HtmlValueEncode(s)` | Encodes a string for inclusion as an HTML element value. It transforms `<`, `>` and `&` to `&lt;`, `&gt;` and `&amp;` correspondingly. | [Example][HtmlValueEncodeExample] |
| `UrlDecode(s)` | Decodes a string taken from an URL. | [Example][UrlDecodeExample] |
| `UrlEncode(s)` | Encodes a string for inclusion in an URL. | [Example][UrlEncodeExample] |
| `XmlDecode(s)` | Decodes a string taken from XML. It transforms `&lt;`, `&gt;`, `&amp;`, `&quot;` and `&apos;` to `<`, `>`, `&`, `"` and `'`  correspondingly. | [Example][XmlDecodeExample] |
| `XmlEncode(s)` | Encodes a string for inclusion in XML. It transforms `<`, `>`, `&`, `"` and `'` to `&lt;`, `&gt;`, `&amp;`, `&quot;` and `&apos;` correspondingly. | [Example][XmlEncodeExample] |

[HtmlAttributeEncodeExample]: Calculator.md?Expression=HtmlAttributeEncode(%22%3Ctag%3E%22)
[HtmlValueEncodeExample]: Calculator.md?Expression=HtmlValueEncode(%22%3Ctag%3E%22)
[UrlDecodeExample]: Calculator.md?Expression=UrlDecode(%22Hello%2bWorld%22)
[UrlEncodeExample]: Calculator.md?Expression=UrlEncode(%22Hello%20World%22)
[XmlDecodeExample]: Calculator.md?Expression=XmlDecode(%22%26lt%3Btag%26gt%3B%22)
[XmlEncodeExample]: Calculator.md?Expression=XmlEncode(%22%3Ctag%3E%22)

#### Markdown-related functions (Waher.Content.Markdown)

The following functions are available in the `Waher.Content.Markdown` library.

| Function | Description | Example |
|----------|-------------|---------|
| `MarkdownEncode(s)` | Encodes a string for inclusion in Markdown. | [Example][MarkdownEncodeExaple] |

[MarkdownEncodeExaple]: Calculator.md?Expression=MarkdownEncode(%22test_sister%22)

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

| Prefix | Name | Scale   |
|:------:|:-----|:-------:|
| Y      | Yotta | 10^24  |
| Z      | Zetta | 10^21  |
| E      | Eta   | 10^18  |
| P      | Peta  | 10^15  |
| T      | Tera  | 10^12  |
| G      | Giga  | 10^9   |
| M      | Mega  | 10^6   |
| k      | Kilo  | 10^3   |
| h      | Hecto | 10^2   |
| da     | Deka  | 10^1   |
| d      | Deci  | 10^-1  |
| c      | Centi | 10^-2  |
| m      | Milli | 10^-3  |
| µ, u   | Micro | 10^-6  |
| n      | Nano  | 10^-9  |
| p      | Pico  | 10^-12 |
| f      | Femto | 10^-15 |
| a      | Atto  | 10^-18 |
| z      | Zepto | 10^-21 |
| y      | Yocto | 10^-24 |

**Note**: When there's an ambiguity of how to interpret a prefix with unit, and the system recognizes a unit with the full name, including the
prefix, the full unit will be chosen. Example: `ft` will be interpreted as *foot*, not *femto-tonnes*, `min` will be interpreted as *minute*,
not *milli-inches*, etc.

### Base Quantities

While any unit can be used to define a physical quantity, unit conversion not based in prefix changes can only be performed on units
recognized by the script engine. Such units are defined in *base quantities*, which are defined in code by creating classes with default
constructors implementing the `Waher.Script.Units.IBaseQuantity` interface. The following tables lists such base quantities as defined by
the `Waher.Script` library:

| Unit | Meaning |
|:----:|:--------|
| m    | Metre |
| Å    | Ångström |
| inch | Inch |
| ft   | Feet |
| foot | Feet |
| yd   | Yard |
| yard | Yard |
| SM   | Statute Mile |
| NM   | Nautical Mile |
[Length]

**Note**: Since `IN` is a keyword, the unit *in* has to be written `inch`.

| Unit | Meaning |
|:----:|:--------|
| g    | Gram |
| t    | Tonne |
| u    | Atomic mass unit |
| lb   | Pound |
[Mass]

| Unit | Meaning |
|:----:|:--------|
| s    | Second |
| min  | Minute |
| h    | Hour |
| d    | Day |
| w    | Week |
[Time]

| Unit | Meaning |
|:----:|:--------|
| A    | Ampere |
[Current]

| Unit | Meaning |
|:----:|:--------|
| °C, C | Celcius |
| °F, F | Farenheit |
| K     | Kelvin |
[Temperature]

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
