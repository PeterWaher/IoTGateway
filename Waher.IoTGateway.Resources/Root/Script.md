Title: Script
Description: Script syntax reference, as understood by the IoT Gateway.
Date: 2016-02-26
Author: Peter Waher
Master: /Master.md

Script syntax reference
=============================

The **IoT Gateway** contains a powerful script parser and evaluation engine. The script engine is not ECMA-compliant. Instead, its focus
is to provide an efficient and compact script language using mathemathical notation. Following is a brief overview of the syntax different 
script elements. You can also use the [Prompt](Prompt.md) to experiment with script syntax.

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

| Variable               | Description                                              |
|:----------------------:|----------------------------------------------------------|
| `Now`                  | Current date and time                                    |
| `Today`                | Current date                                             |

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

Subsets can be creted by allowing the expression in the implicit set definition to be a simple variable reference belonging to a superset, 
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
| `++`          | Post-Increment                              | `a++`        |
| `--`          | Post-Decrement                              | `a--`        |
| `%`           | Percent                                     | `10%`        |
| `‰`           | Per thousand                                | `20‰`        |
| `%0`          | Per thousand                                | `20%0`       |
| `‱`          | Per ten thousand                            | `30‱`       |
| `‰0`          | Per ten thousand                            | `30‰0`       |
| `%00`         | Per ten thousand                            | `30%00`      |
| `°`           | Degrees to radians                          | `sin(100°)`  |
| `'`           | Default differentiation (prim)              | `f'(x)`      |
| `′`           | Default differentiation (prim)              | `f′(x)`      |
| `"`           | Default second-order differentiation (bis)  | `f"(x)`      |
| `″`           | Default second-order differentiation (bis)  | `f″(x)`      |
| `‴`           | Default third-order differentiation         | `f‴(x)`      |
| `T`           | Transposed matrix                           | `M T`        |
| `H`           | Conjugate Transposed matrix                 | `M H`        |
| `†`           | Conjugate Transposed matrix                 | `M†`         |
| `!`           | Faculty                                     | `n!`         |
| `!!`          | Semi-Faculty                                | `n!!`        |
| Physical unit | Defines a physical quantity.                | `10 m/s`     |

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
by comparing both sides. 

Examples:

	[x,y]:=f(a,b,c)
	v[]:=f(a,b,c)

In the first example, the function `f`, which takes three parameters, is supposed to return a vector of two elements. If it does, 
the variables `x` and `y` are assigned the elements of this return vector. In the second example, the `v` is supposed to be a assigned
a vector. If the result of the function call is not a vector, it is converted to a vector before being assigned to `v`.

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
using the `?` operator, followed by the optional `:` operator. There is also a quick null-check statement.

Examples:

	IF Condition THEN IfTrueStatement
	IF Condition THEN IfTrueStatement ELSE IfFalseStatement
	Condition ? IfTrueStatement
	Condition ? IfTrueStatement : IfFalseStatement
	Statement ?? IfNullStatement

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
| `TRY` ... `CATCH` ... `FINALLY` ... | Executes a statement. If an exception occurs, it is caught and an exception statement is executed. Afterwards, a finalization statement is executed. The exception object will be available in the CATCH statement, under the name of `Exception`. | `TRY Statement CATCH Exception FINALLY Done` |
| `TRY` ... `CATCH` ... | Executes a statement. If an exception occurs, it is caught and an exception statement is executed. The exception object will be available in the CATCH statement, under the name of `Exception`. | `TRY Statement CATCH Exception` |
| `TRY` ... `FINALLY` ... | Executes a statement. Afterwards, a finalization statement is executed regardless if an exception has been thrown or not. Any exceptions are automatically propagated. | `TRY Statement FINALLY Done` |
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

| Function | Description | Example |
|----------|-------------|---------|
| `Cos(z)` | Cosine, `z` in radians. | `Cos(100°)` |
| `Cot(z)` | Cotangent, `z` in radians. | `Cot(100°)` |
| `Csc(z)` | Cosecant, `z` in radians. | `Csc(100°)` |
| `Sec(z)` | Secant, `z` in radians. | `Sec(100°)` |
| `Sin(z)` | Sine, `z` in radians. | `Sin(100°)` |
| `Tan(z)` | Tangent, `z` in radians. | `Tan(100°)` |
| `ACos(z)` | Alias for `ArcCos(z)`. | `ACos(Cos(100°))` |
| `ACot(z)` | Alias for `ArcCot(z)`. | `ACot(Cot(100°))` |
| `ACsc(z)` | Alias for `ArcCsc(z)`. | `ACsc(Csc(100°))` |
| `ASec(z)` | Alias for `ArcSec(z)`. | `ASec(Sec(100°))` |
| `ASin(z)` | Alias for `ArcSin(z)`. | `ASin(Sin(100°))` |
| `ATan(z)` | Alias for `ArcTan(z)`. | `ATan(Tan(100°))` |
| `ATan(x,y)` | Alias for `ArcTan(x,y)`. | `ATan(3,4)` |
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

| Function     | Description                               | Example           |
|--------------|-------------------------------------------|-------------------|
| `Abs(z)`     | Absolute value (or magnitude of) `z`      | `Abs(-1)`         |
| `Bool(x)`    | Alias for `Boolean`.                      | `Bool('true')`    |
| `Boolean(x)` | Converts `x` to a boolean value.          | `Boolean('true')` |
| `Ceiling(z)` | Round `z` up to closest integer.          | `Ceiling(pi)`     |
| `Ceil(z)`    | Alias for `Ceiling(z)`.                   | `Ceil(-1)`        |
| `Floor(z)`   | Round `z` down to closest integer.        | `Floor(pi)`       |
| `Max(x,y)`   | Largest of `x` and `y`.                   | `Max(10,a)`       |
| `Min(x,y)`   | Smallest of `x` and `y`.                  | `Min(10,a)`       |
| `Num(x)`     | Alias for `Number(x)`.                    | `Num('100')`      |
| `Number(x)`  | Converts `x` to a number.                 | `Number('100')`   |
| `Round(z)`   | Round `z` up or down to closest integer.  | `Round(pi)`       |
| `Sign(z)`    | Sign of `z` (-1/0/1 + -i/0/+i).           | `Sign(pi)`        |
| `Str(x)`     | Alias for `String(x)`.                    | `Str(100)`        |
| `String(x)`  | Converts `x` to a string.                 | String(100)`      |

### Complex Functions

The following table lists available scalar functions:

| Function | Description | Example |
|----------|-------------|---------|
| `Arg(z)` | Argument (or phase) of `z`. | `Arg(2+i)` |
| `Conj(z)` | Alias for `Conjugate(z)`. | `Conj(2+i)` |
| `Conjugate(z)` | Conjugate of `z`. | `Conjugate(2+i)` |
| `Im(z)` | Imaginary part of `z`. | `Im(2+i)` |
| `Polar(n,φ)` | Complex number given in polar coordinates `n` and `φ`. | `Polar(1,pi/2)` |
| `Re(z)` | Real part of `z`. | `Re(2+i)` |

### String Functions

The following table lists available string-related functions:

| Function | Description | Example |
|----------|-------------|---------|
| `Empty(s)` | Alias for `IsEmpty(s)`. | `Empty(s)` |
| `Eval(s)` | Alias for `Evaluate(s)`. | `Evaluate("a+b")` |
| `Evaluate(s)` | Parses the string and evaluates it. | `Evaluate("a+b")` |
| `IsEmpty(s)` | Returns a boolean value showing if the string `s` is empty or not. | `IsEmpty(s)` |
| `Left(s,N)` | Returns a string with the left-most `N` characters. If the string `s` is shorter, the entire string is returned. | `Left(s,3)` |
| `Len(s)` | Alias for `Length(s)`. | `Len(s)` |
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

| Function      | Description | Example |
|---------------|-------------|---------|
| `And(v)`      | Logical or binary AND of all elements in vector | `And([1,2,3,4,5])`, `And([true,false,true])` |
| `Avg(v)`      | Alias for `Average(v)` | `Avg([1,2,3,4,5])` |
| `Average(v)`  | Average of elements in the vector `v`. | `Average([1,2,3,4,5])` |
| `Count(v)`    | Number of elements in the vector `v`. | `Count([1,2,3,4,5])` |
| `Count(v,x)`  | Number of elements in the vector `v` that are equal to `x`. | `Count([1,2,3,2,1],2)` |
| `Join(v1,v2[,v3[,v4[,v5[,v6[,v7[,v8[,v9]]]]]]])` | Joins a sequence of vectors, into a larger vector. | `Join(v1,v2)` |
| `Left(v,N)`   | Returns a vector with the left-most `N` elements. If the vector `v` is shorter, the entire vector is returned. | `Left(v,3)` |
| `Max(v)`      | The largest element in the vector `v`. | `Max([1,2,3,4,5])` |
| `Median(v)`   | The median element in the vector `v`. | `Median([1,2,3,4,5])` |
| `Mid(v,Pos,Len)` | Returns a vector containing elements from `v`, starting a element `Pos` and continuing `Len` elements. The `Pos` index is zero-based. If the requested vector goes beyond the scope of `v`, the resulting vector gets truncated accordingly. | `Mid(v,5,2)` |
| `Min(v)`      | The smallest element in the vector `v`. | `Min([1,2,3,4,5])` |
| `Nand(v)`     | Logical or binary NAND of all elements in vector | `Nand([1,2,3,4,5])`, `Nand([true,false,true])` |
| `Nor(v)`      | Logical or binary NOR of all elements in vector | `Nor([1,2,3,4,5])`, `Nor([true,false,true])` |
| `Ones(N)`     | Creates an N-dimensional vector with all elements set to 1. | `Ones(5)` |
| `Or(v)`       | Logical or binary OR of all elements in vector | `Or([1,2,3,4,5])`, `Or([true,false,true])` |
| `Prod(v)`     | Alias for `Product(v)` | `Prod([1,2,3,4,5])` |
| `Product(v)`  | Product of elements in the vector `v`. | `Product([1,2,3,4,5])` |
| `Reverse(v)`  | Returns a vector with the elements of the original vector `v` in reverse order. | `Reverse([1,2,3,4,5])` |
| `Right(v,N)`  | Returns a vector with the right-most `N` elements. If the vector `v` is shorter, the entire vector is returned. | `Right(v,3)` |
| `Sort(v[,x1[,x2][,x3][,x4][,x5][,x6][,x7][,x8][,x9]])`   | Sorts a vector `v`. `x1`-`x9` are optional, and can be index values, field names or lambda expressions, and determine how to sort the vector `v`. Negative index numbers, or property of field names beginning with a hyphen `-` are sorted in decending order. Index numbers are one-based, as opposed to normal index values that are zero-based. | `Sort(v,"Field")` |
| `StdDev(v)`   | Alias for `StandardDeviation(v)` | `StdDev([1,2,3,4,5])` |
| `StandardDeviation(v)` | Standard deviation of elements in the vector `v`. | `StandardDeviation([1,2,3,4,5])` |
| `Sum(v)`      | Sum of elements in the vector `v`. | `Sum([1,2,3,4,5])` |
| `Var(v)`      | Alias for `Variance(v)` | `Var([1,2,3,4,5])` |
| `Variance(v)` | Variance of elements in the vector `v`. | `Variance([1,2,3,4,5])` |
| `Xnor(v)`     | Logical or binary XNOR of all elements in vector | `Xnor([1,2,3,4,5])`, `Xnor([true,false,true])` |
| `Xor(v)`      | Logical or binary XOR of all elements in vector | `Xor([1,2,3,4,5])`, `Xor([true,false,true])` |
| `Zeroes(N)`   | Creates an N-dimensional vector with all elements set to 0. | `Zeroes(5)` |

### Matrix Functions

The following functions operate on matrices:

| Function | Description | Example |
|----------|-------------|---------|
| `Identity(N)` | Creates an NxN identity matrix. | `Identity(10)` |
| `Inv(M)` | Alias for `Invert(M)`. | `Inv([[1,1],[0,1]])` |
| `Inverse(M)` | Alias for `Invert(M)`. | `Inverse([[1,1],[0,1]])` |
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
| `Delete(x)` | Alias for `Destroy(x)`. | `Delete(x)` |
| `Destroy(x)` | Destroys the value `x`. If the function references a variable, the variable is also removed. | `Destroy(x)` |
| `Error(Msg)` | Throws an error/exception. | `Error('Something went wrong.')` |
| `Exception(Msg)` | Alias for `Error(Msg)`. | `Exception('Something went wrong.')` |
| `Exists(f)` | Checks if the expression defined by `f` is valid or not. | `Exists(x)` |
| `Fields(x)` | If `x` is a type, `Fields(x)` returns a vector of field names. If `x` is not a type, `Fields(x)` returns a matrix containing field names and values. | `Properties(Ans)` |
| `Methods(x)` | If `x` is a type, `Methods(x)` returns a vector of methods represented as strings. If `x` is not a type, `Methods(x)` returns a matrix containing method names and lambda functions that can be used to execute the corresponding methods. | `Methods(Ans)` |
| `Print(Msg)` | Prints a message to the current console output (which is defined in the variables collection). | `Print(x)` |
| `PrintLine(Msg)` | Prints a message followed by a newline to the current console output. | `PrintLine(x)` |
| `PrintLn(Msg)` | Alias for `PrintLine(Msg)`. | `PrintLine(x)` |
| `Properties(x)` | If `x` is a type, `Properties(x)` returns a vector of property names. If `x` is not a type, `Properties(x)` returns a matrix containing property names and values. | `Properties(Ans)` |
| `Remove(Var)` | Removes the varable `Var` without destroying its contents. | `Remove(x)` |
| `Return(x)` | Returns from the current function scope with the value `x`. | `return(Result)` |

### Extensions

The script engine can be extended by modules that are run in the environment. The following subssections list such funcion extensions
made available in different modules available by default on the gateway. This list does not include funcion extensions made available
by applications that are not part of the **IoT Gateway**.

#### Color functions (Waher.Script.Graphs)

The following functions are available in the `Waher.Script.Graphs` library.

| Function             | Description | Example |
|----------------------|-------------|---------|
| `Alpha(Color,Alpha)` | Sets the Alpha channel of a color. | `Alpha("Red",128)` |
| `Blend(c1,c2,p)`     | Blends colors `c1` and `c2` together using a blending factor 0<=`p`<=1. Any or both of `c1` and `c2` can be an image. | `Blend("Red","Green",0.5)` |
| `Color(string)`      | Parses a string and returns the corresponding color. The color can either be a known color name, or in any of the formats `RRGGBB`, `RRGGBBAA`, `#RRGGBB`, `#RRGGBBAA`. | `Color("Red")`        |
| `GrayScale(Color)`   | Converts a color to its corresponding Gray-scale value. | `GrayScale(cl)`        |
| `HSL(H,S,L)`         | Creates a color from its HSL representation.            | `HSL(100,0.5,0.7)`     |
| `HSLA(H,S,L,A)`      | Creates a color from its HSLA representation.           | `HSLA(100,0.5,0.7,64)` |
| `HSV(H,S,V)`         | Creates a color from its HSV representation.            | `HSV(100,0.5,0.7)`     |
| `HSVA(H,S,V,A)`      | Creates a color from its HSVA representation.           | `HSVA(100,0.5,0.7,64)` |
| `RGB(R,G,B)`         | Creates a color from its RGB representation.            | `RGB(100,150,200)`     |
| `RGBA(R,G,B,A)`      | Creates a color from its RGBA representation.           | `RGBA(100,150,200,64)` |

#### Graph functions (Waher.Script.Graphs)

The following functions are available in the `Waher.Script.Graphs` library. In an interactive script environment, clicking on the resulting graphs
will return a vector corresponding to the point under the mouse.

| Function                                | Description                                           | Example                                   |
|-----------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `HorizontalBars(Labels,Values[,Color])` | Plots a two-dimensional stacked horizontal bar chart. | [Example][HorizontalBarsExample]          |
| `Plot2DArea(X,Y[,Color])`               | Plots a stacked area chart.                           | [Example][Plot2DAreaExample]              |
| `Plot2DCurve(X,Y[,Color[,PenSize]])`    | Plots a smooth two-dimensional curve.                 | [Example][Plot2DCurveExample]             |
| `Plot2DCurveArea(X,Y[,Color])`          | Plots a stacked spline area chart.                    | [Example][Plot2DCurveAreaExample]         |
| `Plot2DLayeredArea(X,Y[,Color])`        | Plots a layered area chart.                           | [Example][Plot2DLayeredAreaExample]       |
| `Plot2DLayeredCurveArea(X,Y[,Color])`   | Plots a layered spline area chart.                    | [Example][Plot2DLayeredCurveAreaExample]  |
| `Plot2DLayeredLineArea(X,Y[,Color])`    | Alias for `Plot2DLayeredArea`.                        | [Example][Plot2DLayeredLineAreaExample]   |
| `Plot2DLayeredSplineArea(X,Y[,Color])`  | Alias for `Plot2DLayeredCurveArea`.                   | [Example][Plot2DLayeredSplineAreaExample] |
| `Plot2DLine(X,Y[,Color[,PenSize]])`     | Alias for `Plot2DCurve`.                              | [Example][Plot2DLineExample]              |
| `Plot2DLineArea(X,Y[,Color])`           | Alias for `Plot2DArea`.                               | [Example][Plot2DLineAreaExample]          |
| `Plot2DSpline(X,Y[,Color[,PenSize]])`   | Plots a smooth two-dimensional curve.                 | [Example][Plot2DSplineExample]            |
| `Plot2DSplineArea(X,Y[,Color])`         | Alias for `Plot2DCurveArea`.                          | [Example][Plot2DSplineAreaExample]        |
| `Polygon2D(X,Y[,Color])`                | Plots a filled polygon.                               | [Example][Polygon2DExample]               |
| `Scatter2D(X,Y[,Color[,BulletSize]])`   | Plots a two-dimensional scatter diagram.              | [Example][Scatter2DExample]               |
| `VerticalBars(Labels,Values[,Color])`   | Plots a two-dimensional stacked vertical bar chart.   | [Example][VerticalBarsExample]            |

[HorizontalBarsExample]: Prompt.md?Expression=x%3A%3D0..20%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3BHorizontalBars(%22x%22%2Bx%2Cy%2Crgba(255%2C0%2C0%2C128))%2BHorizontalBars(%22x%22%2Bx%2Cy2%2Crgba(0%2C0%2C255%2C128))%3B
[Plot2DAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3Bplot2darea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2darea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dline(x%2Cy)%2Bplot2dline(x%2Cy2%2C%22Blue%22)
[Plot2DCurveExample]: Prompt.md?Expression=x:=-10..10|0.1;%0d%0ay:=sin(5*x).*exp(-(x^2/10));%0d%0aplot2dcurve(x,y)
[Plot2DCurveAreaExample]: Prompt.md?Expression=x%3A%3D-10..10%3By%3A%3Dsin(x)%3By2%3A%3D2*sin(x)%3Bplot2dcurvearea(x%2Cy%2Crgba(255%2C0%2C0%2C64))%2Bplot2dcurvearea(x%2Cy2%2Crgba(0%2C0%2C255%2C64))%2Bplot2dcurve(x%2Cy)%2Bplot2dcurve(x%2Cy2%2C%22Blue%22)%2Bscatter2d(x%2Cy%2C%22Red%22%2C5)%2Bscatter2d(x%2Cy2%2C%22Blue%22%2C5)
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

The following table lists variables that control graph output:

| Varaible    | Description                 | Current value              |
|-------------|-----------------------------|----------------------------|
| GraphWidth  | Width of graph, in pixels.  | {try GraphWidth catch ""}  |
| GraphHeight | Height of graph, in pixels. | {try GraphHeight catch ""} |

The following table lists properties on 2D-graph object that can be used to control how the graph is rendered:

| Property  | Type    | Description                      | Default value |
|-----------|---------|----------------------------------|---------------|
| ShowXAxis | Boolean | If the x-axis is to be displayed | `true`        |
| ShowYAxis | Boolean | If the y-axis is to be displayed | `true`        |
| ShowGrid  | Boolean | If the grid is to be displayed   | `true`        |

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
| `NovaMandelbrotFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | Calculates a Nova-Mandelbrot fractal. | `NovaMandelbrotFractal(0,0,0.1,0,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaMandelbrotSmoothFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaMandelbrotFractal`, except the image is smoothed out using the *Heat Equation*. Pixels where colors change are used as fixed boundary conditions. | `NovaMandelbrotSmoothFractal(0,0,0.1,0,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |
| `NovaMandelbrotTopographyFractal(r,i,dr,R,p[,Palette[,DimX[,DimY]]])` | As `NovaMandelbrotFractal`, except only pixels where the color changes are returned, creating a topographical map of the image. | `NovaMandelbrotTopographyFractal(0,0,0.1,0,3,0.5,5.2,randomlinearrgb(1024,16),640,480)` |

#### Iterated Function System (IFS) Fractal functions (Waher.Script.Fractals)

The following functions can be used to create fractal images based on Iterated Function Systems (IFS). The functions are available in the 
`Waher.Script.Fractals` library. They can be used as a means to create backgound images for themes, etc.

| Function                                | Description                                           | Example                                   |
|-----------------------------------------|-------------------------------------------------------|-------------------------------------------|
| `FlameFractalHsl(xc,yc,dr,N,f[,Preview[,Parallel[,DimX[,DimY[,SuperSampling[,Gamma[,LightFactor[,Seed]]]]]]]])` | Calculates a flame fractal in HSL space. Intensity is mapped along the L-axis. Gamma correction is done along the SL-axes. The L-axis is multiplicated with the LightFactor. | `FlameFractalHsl(0.6109375,0.199208333333333,0.625,1e7,[Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",Identity(2),DiamondVariation(),"Red"],False,False,400,300,1,2.5,2,1668206157)` |
| `FlameFractalRgba(xc,yc,dr,N,f[,Preview[,Parallel[,DimX[,DimY[,SuperSampling[,Gamma[,Vibrancy[,Seed]]]]]]]])` | Calculates a flame fractal in RGBA space. Intensity is calculated along the A-axis. Gamma correction is done along the RGB-axes (vibrancy=0) or along the A-axis (vibrancy=1), or a combination thereof. | `FlameFractalRgba(0,0,0,1e7,[Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",ExponentialVariation(),Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Red",ExponentialVariation()],400,300)` |
| `IfsFractal(xc,yc,dr,N,T[,DimX[,DimY[,Seed]]])` | Calculates a fractal based on an Iterated Function System, using the chaos game. | ` IfsFractal(0,5,6,2e6,[[[0,0,0],[0,0.16,0],[0,0,1]],0.01,"Green",[[0.85,0.04,0],[-0.04,0.85,1.6],[0,0,1]],0.85,"Green",[[0.2,-0.26,0],[0.26,0.24,1.6],[0,0,1]],0.07,"Green",[[-0.15,0.28,0],[0.26,0.24,0.44],[0,0,1]],0.07,"Green"],300,600);` |

`IfsFractals` run on Iterated Function Systems using systems of linear equations. Flame fractals can modify the linear transforms using one or more
*variations*. There are many different types of variations[^For more information about Flame Fractals and variations, see the paper on
<a href="http://flam3.com/flame.pdf" target="_blank">The Fractal Flame Algorithm</a> by Scott Daves and Erik Reckase] that can be used:

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

#### Persistence-related functions (Waher.Script.Persistence)

The following functions are available in the `Waher.Script.Persistence` library.

| Function | Description | Example |
|----------|-------------|---------|
| `DeleteObject(Obj)` | Deletes an object from the underlying persistence layer. | `Delete(Obj)` |
| `FindObjects(Type, Offset, MaxCount, Filter, SortOrder)` | Finds objects of a given `Type`. `Offset` and `MaxCount` provide a means to paginate the result set. `Filter` can be null, if none is used, or a string containing an expression to limit the result set. `SortOrder` sorts the result. It also determines the index to use. | `FindObjects(Namespace.CustomType, 0, 10, "StringProperty='StringValue'", ["Property1","Property2"])` |
| `SaveNewObject(Obj)` | Saves a new object to the underlying persistence layer. | `SaveNewObject(Obj)` |
| `UpdateObject(Obj)` | Updaes an object in the underlying persistence layer. | `UpdateObject(Obj)` |

#### Statistics-related functions (Waher.Script.Statistics)

The following functions are available in the `Waher.Script.Statistics` library.

| Function | Description | Example |
|----------|-------------|---------|
| `Beta(Alpha,Beta[,N]])`      | Generates a random number using the Beta distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Beta(2,5,10000),0,1,10);VerticalBars(Labels,Counts)` |
| `Cauchy(Median,Scale[,N]])`  | Generates a random number using the Cauchy distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Cauchy(5,1.5,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Chi2(Degrees[,N]])`         | Generates a random number using the Chi squared distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Chi2(6,10000),0,20,10);VerticalBars(Labels,Counts)` |
| `Exponential([Mean[,N]])`    | Generates a random number using the Exponential distribution. If no `Mean` is given, the mean is assumed to be 1. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Exponential(3,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Gamma(Shape,Scale[,N]])`    | Generates a random number using the Gamma distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Gamma(3,3,10000),0,20,10);VerticalBars(Labels,Counts)` |
| `Histogram(V,Min,Max,N)`     | Calculates the histogram of a set of data `V` with `N` buckets between `Min` and `Max`. | `[Labels,Counts]:=Histogram(Uniform(0,10,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Laplace(Mean,Scale[,N]])`   | Generates a random number using the Laplace distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Laplace(5,1.5,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Normal([Mean,StdDev][,N]])` | Generates a random number using the Normal distribution. If no `Mean` and standard deviation `StdDev` is given, the mean is assumed to be 0 and standarddeviation assumed to be 1. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Normal(0,5,10000),-20,20,10);VerticalBars(Labels,Counts)` |
| `StudentT(Degrees[,N]])`     | Generates a random number using the Student-T distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(StudentT(6,10000),-5,5,10);VerticalBars(Labels,Counts)` |
| `Uniform([Min,Max][,N]])`    | Generates a random number using the Uniform distribution. If no interval is given, the standard interval [0,1] is assumed. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Uniform(0,10,10000),0,10,10);VerticalBars(Labels,Counts)` |
| `Weibull(Shape,Scale[,N]])`  | Generates a random number using the Weibull distribution. If `N` is provided, a vector with random elements is returned. | `[Labels,Counts]:=Histogram(Weibull(5,3,10000),0,10,10);VerticalBars(Labels,Counts)` |

#### Content-related functions (Waher.Script.Content)

The following functions are available in the `Waher.Script.Content` library.

| Function                        | Description | Example |
|---------------------------------|-------------|---------|
| `Base64Decode(Data)`            | Decodes binary data from a string using BASE64 encoding. | [Example][Base64DecodeExample] |
| `Base64Encode(Data)`            | Encodes binary data to a string using BASE64 encoding. | [Example][Base64EncodeExample] |
| `Base64UrlDecode(Data)`         | Decodes binary data from a string using BASE64URL encoding. | [Example][Base64UrlDecodeExample] |
| `Base64UrlEncode(Data)`         | Encodes binary data to a string using BASE64URL encoding. | [Example][Base64UrlEncodeExample] |
| `Decode(Content,Type)`          | Decodes `Content` using the available Internet Content Type decoder for Content Type `Type`. | [Example][DecodeExample] |
| `Encode(Object[,Types])`        | Encodes `Object` using the available Internet Content Type encoders. If `Types` is provided, it is an array of acceptable content types that can be used. The result is a two-dimensional vector, containing the binary encoding as the first element, and the content type as the second element. | [Example][EncodeExample] |
| `Get(Url[,Accept/Headers])`     | Retrieves a resource, in accordance with the [URI scheme](#uriSchemes) of the `Url`, and decodes it, in accordance with its content type. If a second argument is provided, it either represents an `Accept` header, if a string, or custom protocol-specific headers or options, if an object. | [Example][GetExample] |
| `HtmlAttributeEncode(s)`        | Encodes a string for inclusion in an HTML attribute. It transforms `<`, `>`, `&` and `"` to `&lt;`, `&gt;`, `&amp;` and `&quot;` correspondingly. | [Example][HtmlAttributeEncodeExample] |
| `HtmlValueEncode(s)`            | Encodes a string for inclusion as an HTML element value. It transforms `<`, `>` and `&` to `&lt;`, `&gt;` and `&amp;` correspondingly. | [Example][HtmlValueEncodeExample] |
| `LoadFile(FileName)`            | Loads a file and decodes it, in accordance with its file extension. | [Example][LoadFileExample] |
| `SaveFile(Obj,FileName)`        | Encodes an object `Obj` in accordance with its type and file extension, and saves it as a file. | [Example][SaveFileExample] |
| `UrlDecode(s)`                  | Decodes a string taken from an URL. | [Example][UrlDecodeExample] |
| `UrlEncode(s)`                  | Encodes a string for inclusion in an URL. | [Example][UrlEncodeExample] |
| `XmlDecode(s)`                  | Decodes a string taken from XML. It transforms `&lt;`, `&gt;`, `&amp;`, `&quot;` and `&apos;` to `<`, `>`, `&`, `"` and `'`  correspondingly. | [Example][XmlDecodeExample] |
| `XmlEncode(s)`                  | Encodes a string for inclusion in XML. It transforms `<`, `>`, `&`, `"` and `'` to `&lt;`, `&gt;`, `&amp;`, `&quot;` and `&apos;` correspondingly. | [Example][XmlEncodeExample] |

[Base64DecodeExample]: Prompt.md?Expression=Decode(Base64Decode("SGVsbG8="),"text/plain")
[Base64EncodeExample]: Prompt.md?Expression=Base64Encode(Encode("Hello")[0])
[Base64UrlDecodeExample]: Prompt.md?Expression=Decode(Base64UrlDecode("SGVsbG8"),"text/plain")
[Base64UrlEncodeExample]: Prompt.md?Expression=Base64UrlEncode(Encode("Hello")[0])
[DecodeExample]: Prompt.md?Expression=Decode(Csv,%22text/csv%22)
[EncodeExample]: Prompt.md?Expression=Encode("Hello",[%22text/plain%22])
[GetExample]: Prompt.md?Expression=Get(%22URL%22)
[HtmlAttributeEncodeExample]: Prompt.md?Expression=HtmlAttributeEncode(%22%3Ctag%3E%22)
[HtmlValueEncodeExample]: Prompt.md?Expression=HtmlValueEncode(%22%3Ctag%3E%22)
[LoadFileExample]: Prompt.md?Expression=LoadFile(%22FileName%22)
[SaveFileExample]: Prompt.md?Expression=SaveFile(Graph,%22Graph.png%22)
[UrlDecodeExample]: Prompt.md?Expression=UrlDecode(%22Hello%2bWorld%22)
[UrlEncodeExample]: Prompt.md?Expression=UrlEncode(%22Hello%20World%22)
[XmlDecodeExample]: Prompt.md?Expression=XmlDecode(%22%26lt%3Btag%26gt%3B%22)
[XmlEncodeExample]: Prompt.md?Expression=XmlEncode(%22%3Ctag%3E%22)

#### XML-related functions (Waher.Script.Xml)

The following functions are available in the `Waher.Script.Xml` library.

| Function                        | Description | Example |
|---------------------------------|-------------|---------|
| `Xml(s)`                        | Converts the string `s` to an XML Document. | `Xml("<a>Hello</a>")` |

#### Markdown-related functions (Waher.Content.Markdown)

The following functions are available in the `Waher.Content.Markdown` library.

| Function                 | Description | Example |
|--------------------------|-------------|---------|
| `LoadMarkdown(FileName)` | Loads a markdown file and preprocesses it before returning it as a string. | [Example][LoadMarkdownExample] |
| `MarkdownEncode(s)`      | Encodes a string for inclusion in Markdown. | [Example][MarkdownEncodeExample] |
| `PreprocessMarkdown(MD)` | Preprocesses a markdown string `MD`, and returns it as a string. | [Example][PreprocessMarkdownExample] |

The following predefined context-specific constants (read-only variables) are available in inline script:

| Variable        | Description                                                           |
|:---------------:|-----------------------------------------------------------------------|
| `StartPosition` | The starting position of the current script in the markdown document. |
| `EndPosition`   | The ending position of the script in the markdown document.           |

[LoadMarkdownExample]: Prompt.md?Expression=LoadMarkdown(%22File.md%22)
[MarkdownEncodeExample]: Prompt.md?Expression=MarkdownEncode(%22test_sister%22)
[PreprocessMarkdownExample]: Prompt.md?Expression=s%3A%3D%22Hello%20World%21%22%3BPreprocessMarkdown%28%22%2A%7B%7Bs%7D%7D%2A%22%29

#### XSL-related functions (Waher.Content.Xsl)

The following functions are available in the `Waher.Content.Xsl` library.

| Function                 | Description | Example |
|--------------------------|-------------|---------|
| `Transform(XML,XSLT)`    | Transforms an XML document using an XSL Transform (XSLT). | [Example][TransformExample] |

[TransformExample]: Prompt.md?Expression=Transform(LoadFile(%22Data.xml%22),LoadFile(%22Transform.xslt%22))

#### Web Extensions (Waher.Networking.HTTP\[.UWP\])

Script can be embeded in transformable web content, such as [Markdown documents](/Markdown.md#script). The following functions are 
available in the `Waher.Networking.HTTP` and `Waher.Networking.HTTP.UWP` libraries.

| Function                          | Description | Example |
|-----------------------------------|-------------|---------|
| `HttpError(Code,Message,Content)` | Returns an HTTP Error to the client y throwing an `HttpException` exception. | `HttpError(400,"Bad Request","Missing parameters.")` |

In the following subsections, specialized HTTP Error functions are listed.

##### Redirections

The following functions return HTTP redirection responses back to he client:

| Function                    | Code | Description |
|-----------------------------|-----:|-------------|
| Found(Location)             |  302 | Returns the Found redirection back to the client.              |
| MovedPermanently(Location)  |  301 | Returns the Moved Permanently redirection back to the client.  |
| NotModified()               |  304 | Returns the Not Modified message back to the client.           |
| SeeOther(Location)          |  303 | Returns the See Other redirection back to the client.          |
| TemporaryRedirect(Location) |  307 | Returns the Temporary Redirect redirection back to the client. |
| UseProxy(Location)          |  305 | Returns the Use Proxy redirection back to the client.          |

##### Client Errors

The following functions return HTTP client error responses back to he client:

| Function                            | Code | Description |
|-------------------------------------|-----:|-------------|
| BadRequest(Content)                 |  400 | Returns the Bad Request client error message back to the client.                   |
| Conflict(Content)				      |  409 | Returns the Conflict client error message back to the client.                      |
| FailedDependency(Content)		      |  424 | Returns the Failed Dependency client error message back to the client.             |
| Forbidden(Content)                  |  403 | Returns the Forbidden client error message back to the client.                     |
| Gone(Content)					      |  410 | Returns the Gone client error message back to the client.                          |
| Locked(Content)				      |  423 | Returns the Locked client error message back to the client.                        |
| MisdirectedRequest(Content)	      |  421 | Returns the Misdirected Request client error message back to the client.           |
| NotAcceptable(Content)			  |  406 | Returns the Not Acceptable client error message back to the client.                |
| NotFound(Content)				      |  404 | Returns the Not Found client error message back to the client.                     |
| PreconditionFailed(Content)	      |  411 | Returns the Precondition Failed client error message back to the client.           |
| PreconditionRequired(Content)	      |  428 | Returns the Precondition Required client error message back to the client.         |
| RangeNotSatisfiable(Content)	      |  416 | Returns the Range Not Satisfiable client error message back to the client.         |
| RequestTimeout(Content)		      |  408 | Returns the Request Timeout client error message back to the client.               |
| TooManyRequests(Content)		      |  429 | Returns the Too Many Requests client error message back to the client.             |
| UnavailableForLegalReasons(Content) |  451 | Returns the Unavailable For Legal Reasons client error message back to the client. |
| UnprocessableEntity(Content)	      |  422 | Returns the Unprocessable Entity client error message back to the client.          |
| UnsupportedMediaType(Content)       |  415 | Returns the Unsupported Media Type client error message back to the client.        |

##### Server Errors

The following functions return HTTP server error responses back to he client:

| Function                               | Code | Description |
|----------------------------------------|-----:|-------------|
| BadGateway(Content)                    |  502 | Returns the Bad Gateway server error message back to the client.                   |
| GatewayTimeout(Content)                |  504 | Returns the Gateway Timeout server error message back to the client.                   |
| InsufficientStorage(Content)           |  507 | Returns the Insufficient Storage server error message back to the client.                   |
| InternalServerError(Content)           |  500 | Returns the Internal Server Error server error message back to the client.                   |
| LoopDetected(Content)                  |  508 | Returns the Loop Detected server error message back to the client.                   |
| NetworkAuthenticationRequired(Content) |  511 | Returns the Network Authentication Required server error message back to the client.                   |
| NotExtended(Content)                   |  510 | Returns the Not Extended server error message back to the client.                   |
| NotImplemented(Content)                |  501 | Returns the Not Implemented server error message back to the client.                   |
| ServiceUnavailable(Content)            |  503 | Returns the Service Unavailable server error message back to the client.                   |
| VariantAlsoNegotiates(Content)         |  506 | Returns the Variant Also Negotiates server error message back to the client.                   |

The following predefined variables are available in such web content files:

| Variable       | Description                                              |
|:--------------:|----------------------------------------------------------|
| `Request`      | The current HttpRequest object.                          |
| `Response`     | The current HttpResponse object.                         |
| `Posted`       | Any decoded data posted to the resource.                 |
| `Gloabl`       | Global variables.                                        |
| `Page`         | Page-local variables.                                    |

#### Gateway Extensions (Waher.IoTGateway)

The following predefined variables are available in web pages hosted by the IoT Gateway:

| Variable       | Description                                              |
|:--------------:|----------------------------------------------------------|
| `Language`     | The language object of the current session.              |
| `Namespace`    | The language namespace object of the current page.       |

#### Serialization-related functions (Waher.Service.NeuroLedger)

The following functions are available in the `Waher.Service.NeuroLedger` library, which is part of the Neuro-Ledger^TM.

| Function                           | Description                                                                                                                                                                                          | Example |
|------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------|
| `Deserialize(Bin[,BaseType])       | Deserializes a byte array into an object, or vector of objects. If serialization does not include type information, a base type can be provided. If not, a generic deserializer will be used.        | `Deserialize(Base64Decode(s))` |
| `FromBinary(Bin[,BaseType])        | Alias for `Deserialize`.                                                                                                                                                                             | `FromBinary(Base64Decode(s))` |
| `Serialize(Object)`                | Serializes an object to a byte array.                                                                                                                                                                | `Base64Encode(Serialize(Obj))` |
| `Serialize(Vector)`                | Serializes a vector of objects to a byte array.                                                                                                                                                      | `Base64Encode(Serialize([Obj1,Obj2,Obj3]))` |
| `PrintDeserialize(Bin[,BaseType])` | Works as `Deserialize(Bin[,BaseType])`, except that individual elements are printed to the standard output. This can be used for debugging purposes when transporting serializations across systems. | `PrintDeserialize(Base64Decode(s))` |
| `PrintSerialize(Object)`           | Works as `Serialize(Object)`, except that individual elements are printed to the standard output. This can be used for debugging purposes when transporting serializations across systems.           | `Base64Encode(PrintSerialize(Obj))` |
| `PrintSerialize(Vector)`           | Works as `Serialize(Vector)`, except that individual elements are printed to the standard output. This can be used for debugging purposes when transporting serializations across systems.           | `Base64Encode(PrintSerialize([Obj1,Obj2,Obj3]))` |
| `ToBinary(Object)`                 | Alias for `Serialize`.                                                                                                                                                                               | `Base64Encode(ToBinary(Obj))` |
| `ToBinary(Vector)`                 | Alias for `Serialize`.                                                                                                                                                                               | `Base64Encode(ToBinary([Obj1,Obj2,Obj3]))` |

=========================================================================================================================================================

URI Schemes
------------------

URI Schemes recognized by the system depends on what modules are loaded. Classes with default constructors, implementing the
`Waher.Content.IContentGetter` interface will automatically be used when resources are requested using corresponding URI schemes.
The following table lists recognized URI schemes:

| URI Scheme | Module                        | Description |
|:-----------|:------------------------------|:------------|
| `http`     | `Waher.Content`               | Resource retrieved using the HTTP protocol. |
| `https`    | `Waher.Content`               | Resource retrieved using the HTTPS protocol (HTTP+SSL/TLS). |
| `httpx`    | `Waher.Networking.XMPP.HTTPX` | Resource retrieved using the [XEP-0332: HTTP over XMPP transport](https://xmpp.org/extensions/xep-0332.html) protocol. |


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
SELECT [TOP maxcount] [DISTINCT]
	* |
	column1 [name1][, column2 [name2][, ...]]
FROM
	type1[, type2[, ...]]
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

##### Vector Sources

The `SELECT` statement can search for information from different types of sources. These
sources are defined in the `FROM` clause. If they point to Type Names, they refer to
objects of the specified type that are stored in the object database. The sources can also
be script that returns any type of vector. In such a case, the `SELECT` statement operates
directly on these vectors, without going to the database.

Example:



#### UPDATE

Simplified SQL `UPDATE` statements can be executed against the object database. The number of objects updated is returned.

Syntax:

```
UPDATE
	type
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
DELETE
FROM
	type
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

### XML

The `Waher.Script.Xml` library extends the script engine to understand XML embedded in the script.
Attribute values are always considered to be script. You can provide constant strings, as usual,
but also provide script to dynamically populate your XML document with contents and calculations
based on current variable values. In element text values, you can also embed script between the
special `<[` and `]>` operators, or the corresponding `<(` and `)>` operators. Element names and
attribute names are always interpreted literally. If you need to create dynamic XML, you can build
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
