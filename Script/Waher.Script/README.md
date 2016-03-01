Introduction
=============

The **Waher.Script** library contains a script parser and evaluation engine written in C#. The script engine is not ECMA-compliant. Instead, its focus
is to provide an efficient and compact script language using mathemathical notation.
It's part of the [IoTGateway solution](https://github.com/PeterWaher/IoTGateway).

Script syntax
==============

Following is a brief overview of the syntax different script elements.

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

| Operator     | Description                                 | Example      |
|:------------:|:--------------------------------------------|:------------:|
| `.`          | Member operator                             | `obj.Member` |
| `(` List `)` | Function evaluation                         | `f(a,b,c)`   |
| `[]`         | To vector, if not already                   | `a[]`        |
| `[Index]`    | Vector index operator                       | `v[i]`       |
| `[X,Y]`      | Matrix index operator                       | `M[x,y]`     |
| `[X,]`       | Matrix colum vector operator                | `M[x,]`      |
| `[,Y]`       | Matrix row vector operator                  | `M[,y]`      |
| `[,]`        | To matrix, if not already                   | `a[,]`       |
| `{}`         | To set, if not already                      | `a{}`        |
| `++`         | Post-Increment                              | `a++`       |
| `--`         | Post-Decrement                              | `a--`       |
| `%`          | Percent                                     | `10%`       |
| `‰`          | Per thousand                                | `20‰`       |
| `‱`        | Per ten thousand                           | `30‱`     |
| `°`          | Degrees to radians                          | `sin(100°)` |
| `'`          | Default differentiation (prim)              | `f'(x)`     |
| `′`          | Default differentiation (prim)              | `f′(x)`     |
| `"`          | Default second-order differentiation (bis)  | `f"(x)`     |
| `″`          | Default second-order differentiation (bis)  | `f″(x)`     |
| `‴`          | Default third-order differentiation         | `f‴(x)`     |
| `T`          | Transposed matrix                           | `M T`       |
| `H`          | Conjugate Transposed matrix                 | `M H`       |
| `†`          | Conjugate Transposed matrix                 | `M†`        |
| `!`          | Faculty                                     | `n!`        |
| `!!`         | Semi-Faculty                                | `n!!`       |

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
using the `?` operator, followed by the optional optional `:` operator. Examples:

	IF Condition THEN IfTrueStatement
	IF Condition THEN IfTrueStatement ELSE IfFalseStatement
	Condition ? IfTrueStatement
	Condition ? IfTrueStatement : IfFalseStatement

**Note**: `IF`, `THEN` and `ELSE` are case insensitive. They are written here using upper case for clarity.

**Note 2**: If no `ELSE` or `:` is present, the statement is evaluated to **null**.

**Note 3**: The `THEN` keyword is optional, and can be omitted.

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

### Statements (DO/WHILE, WHILE/DO, FOR, FOREACH, TRY CATCH FINALLY)

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

**Note**: The use of the `DO` keyword is optional, except in the `DO`-`WHILE` case. It can be omitted, or replaced by a `:`. 

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

### Vector Functions

The following functions operate on vectors:

| Function | Description | Example |
|----------|-------------|---------|
| `And(v)` | Logical or binary AND of all elements in vector | `And([1,2,3,4,5])`, `And([true,false,true])` |
| `Avg(v)` | Same as `Average(v)` | `Avg([1,2,3,4,5])` |
| `Average(v)` | Average of elements in the vector `v`. | `Average([1,2,3,4,5])` |
| `Max(v)` | The largest element in the vector `v`. | `Max([1,2,3,4,5])` |
| `Median(v)` | The median element in the vector `v`. | `Median([1,2,3,4,5])` |
| `Min(v)` | The smallest element in the vector `v`. | `Min([1,2,3,4,5])` |
| `Nand(v)` | Logical or binary NAND of all elements in vector | `Nand([1,2,3,4,5])`, `Nand([true,false,true])` |
| `Nor(v)` | Logical or binary NOR of all elements in vector | `Nor([1,2,3,4,5])`, `Nor([true,false,true])` |
| `Or(v)` | Logical or binary OR of all elements in vector | `Or([1,2,3,4,5])`, `Or([true,false,true])` |
| `Prod(v)` | Same as `Product(v)` | `Prod([1,2,3,4,5])` |
| `Product(v)` | Product of elements in the vector `v`. | `Product([1,2,3,4,5])` |
| `StdDev(v)` | Same as `StandardDeviation(v)` | `StdDev([1,2,3,4,5])` |
| `StandardDeviation(v)` | Standard deviation of elements in the vector `v`. | `StandardDeviation([1,2,3,4,5])` |
| `Sum(v)` | Sum of elements in the vector `v`. | `Sum([1,2,3,4,5])` |
| `Var(v)` | Same as `Variance(v)` | `Var([1,2,3,4,5])` |
| `Variance(v)` | Variance of elements in the vector `v`. | `Variance([1,2,3,4,5])` |
| `Xnor(v)` | Logical or binary XNOR of all elements in vector | `Xnor([1,2,3,4,5])`, `Xnor([true,false,true])` |
| `Xor(v)` | Logical or binary XOR of all elements in vector | `Xor([1,2,3,4,5])`, `Xor([true,false,true])` |

### Matrix Functions

The following functions operate on matrices:

| Function | Description | Example |
|----------|-------------|---------|
| `Inv(M)` | Same as `Invert(M)`. | `Inv([[1,1],[0,1]])` |
| `Inverse(M)` | Same as `Invert(M)`. | `Inverse([[1,1],[0,1]])` |
| `Invert(M)` | Inverts `M`. Works on any invertable element. | `Invert([[1,1],[0,1]])` |

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


License
==============

The source code provided in this project is provided open for the following uses:

* For **Personal evaluation**. Personal evaluation means evaluating the code, its libraries and underlying technologies, including learning 
	about underlying technologies.

* For **Academic use**. If you want to use the following code for academic use, all you need to do is to inform the author of who you are, what academic
	institution you work for (or study for), and in what projects you intend to use the code. All I ask in return is for an acknowledgement and
	visible attribution to this project.

* For **Security analysis**. If you perform any security analysis on the code, to see what security aspects the code might have,
	all I ask is that you inform me of any findings so that any vulnerabilities might be addressed. I am thankful for any such contributions,
	and will acknowledge them.

All rights to the source code are reserved. If you're interested in using the source code, as a whole, or partially, you need a license agreement
with the author. You can contact him through [LinkedIn](http://waher.se/).

This software is provided by the copyright holders and contributors "as is" and any express or implied warranties, including, but not limited to, 
the implied warranties of merchantability and fitness for a particular purpose are disclaimed. In no event shall the copyright owner or contributors 
be liable for any direct, indirect, incidental, special, exemplary, or consequential damages (including, but not limited to, procurement of substitute 
goods or services; loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether in contract, strict 
liability, or tort (including negligence or otherwise) arising in any way out of the use of this software, even if advised of the possibility of such 
damage.

[order of presedence]: https://en.wikipedia.org/wiki/Order_of_operations