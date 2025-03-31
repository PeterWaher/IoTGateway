Title: ChunkedList Performance
Description: Contains Benchmarking results for ChunkedList in comparison to LinkedList and List
Author: Peter Waher
Date: 2025-03-29
Master: /Master.md

================================================

ChunkedList Performance
==========================

The `ChunkedList<T>` is a generic data structure that is optimized for performance. It combines 
features from the `LinkedList<T>` structure, with features of the `List<T>` structure. 

The `LinkedList<T>` structure generates nodes for each element, and arranges them in a linked 
list. It is very straight forward to add and remove elements. But the structure requires a lot 
of  memory overhead, especially if storing small elements. It is also inefficient to process, as
elements are not stored closely together, not allowing processor caches to be utilized 
efficiently.

The `List<T>` structure on the other hand, stores all elements in a single array internally.
This makes access much faster. But adding elements can require the internal buffer to be
resized, which can be a costly operation. To avoid this, the size of the increase grows as 
elements are added (i.e. size is doubled).

The `ChunkedList<T>` combines the two structures. Instead of attempting to have a single
internal array, which requires resizing, the list creates chunks, and links the chunks together
in a linked list of chunks. This allows for elements within a chunk to be stored closely 
together, while avoiding the need to resize the internal buffer, avoiding the necessary copying
of data each time that happens.

Following are some performance benchmarks done, comparing the performance of the 
`ChunkedList<T>` structure, with the `LinkedList<T>` and `List<T>` structures. The
`ChunkedList<T>` structure has comparable interfaces, allowing it to fit in code that
uses either of the `LinkedList<T>` or `List<T>` structures. You can find the code of the
`ChunkedList<T>` structure in the [IoTGateway repository](https://github.com/PeterWaher/IoTGateway/tree/master/Runtime/Waher.Runtime.Collections).
The same repository also contains unit tests and the following [benchmarking tests](https://github.com/PeterWaher/IoTGateway/tree/master/Runtime/Waher.Runtime.Collections.Test).

The following benchmarks were done on a Windows 11 Intel i9 laptop machine, in Release mode.
Different performance results may occur on different types of machines, and in different
build modes, and for different processors. The benchmarks tests are done first on a small
number of elements, to test how the structure works for small number of elements, and then
does the same thing for a larger set of elements. The times are then compared to compute a
relative performance (in percent) between the `ChunkedList<T>` and the `LinkedList<T>` and
`List<T>` structures.

For all graphs, the x-axis represents the number of elements added, and the y-axis represents
the relative performance, in percent. An additional black line parallel to the x-axis marks the
100% level, where the lists perform equally. If the colored graph lies above this line, the
`ChunkedList<T>` structure performs better than the structure represented by the graph. If the 
colored graph lies below the line, the `ChunkedList<T>` structure performs worse. The hidden 
implementations explain the variations of the curves, as memory exceed internal caches, or 
buffers have to be resized. The graphs are colored as follows:

![Legend](LegendRel.png)

Adding elements (last)
-------------------------

Simple addition of elements (last in the list):

![Adding small amount of elements](GRel_Test_01_Add_Small.png 600)
![Adding large amount of elements](GRel_Test_02_Add_Large.png 600)

Enumerating elements using an enumerator
-------------------------------------------

Enumerating elements in a list sequentially:

![Enumerating small amount of elements](GRel_Test_03_Enumerate_Small.png 600)
![Enumerating large amount of elements](GRel_Test_04_Enumerate_Large.png 600)

Enumerating elements using `ForEach`
---------------------------------------

Enumerating elements in a list sequentially using `ForEach()` (not available in `LinkedList`, 
so enumeration is performed using an enumerator):

![Enumerating small amount of elements using ForEach](GRel_Test_05_ForEach_Small.png 600)
![Enumerating large amount of elements using ForEach](GRel_Test_06_ForEach_Large.png 600)

Enumerating elements using `ForEachChunk`
-------------------------------------------

Enumerating elements in a list sequentially using `ForEachChunk()`, giving the `ChunkedList<T>`
an edge. (Not available in `LinkedList`, so enumeration is performed using an enumerator,
or `List`, where enumeration is done using `ForEach()`):

![Enumerating small amount of elements using ForEachChunk](GRel_Test_07_ForEachChunk_Small.png 600)
![Enumerating large amount of elements using ForEachChunk](GRel_Test_08_ForEachChunk_Large.png 600)

Checking if list contains en element
---------------------------------------

The `Contains()` method is used to check if a list contains an element:

![Checking if list contains an element](GRel_Test_09_Contains_Small.png 600)
![Checking if list contains an element](GRel_Test_10_Contains_Large.png 600)

Removing elements randomly
-----------------------------

Removing elements randomly using the `Remove()` method:

![Removing elements randomly](GRel_Test_11_Remove_Small.png 600)
![Removing elements randomly](GRel_Test_12_Remove_Large.png 600)

Removing first elements
--------------------------

Removing first elements using the `RemoveFirst()` method (or `RemoveAt()` for `List`):

![Removing first elements](GRel_Test_13_RemoveFirst_Small.png 600)
![Removing first elements](GRel_Test_14_RemoveFirst_Large.png 600)

**Note**: The `ChunkedList<T>` structure outperforms the `LinkedList<T>` structure, even for
this type of linked-list operation.

**Note 2**: The `List<T>` structure quickly diverges in performance for this operation. It is
not suitable for implementing a FIFO queue, for instance.

Removing last elements
-------------------------

Removing last elements using the `RemoveLast()` method (or `RemoveAt()` for `List`):

![Removing last elements](GRel_Test_15_RemoveLast_Small.png 600)
![Removing last elements](GRel_Test_16_RemoveLast_Large.png 600)

**Note**: The `ChunkedList<T>` structure outperforms the `LinkedList<T>` structure, even for
this type of linked-list operation.

Adding elements (first)
--------------------------

Simple addition of elements first in the list (or using `Insert()` for `List`):

![Adding small amount of elements first](GRel_Test_17_AddFirst_Small.png 600)
![Adding large amount of elements first](GRel_Test_18_AddFirst_Large.png 600)

**Note**: The `ChunkedList<T>` structure outperforms the `LinkedList<T>` structure, even for
this type of linked-list operation.

**Note 2**: The `List<T>` structure quickly diverges in performance for this operation. It is
not suitable for implementing queues or priority queues, for instance, where you process items
from either end of the list.

Index operations (get/set)
-----------------------------

Index operations using `this[Index]` is not available in the `LinkedList<T>` structure, so
the benchmark comparison is only done between the `ChunkedList<T>` and `List<T>` structures:

![Index operations](GRel_Test_19_Index_Small.png 600)
![Index operations](GRel_Test_20_Index_Large.png 600)

Finding elements
-------------------

Finding elements uses the `IndexOf` method for for `ChunkedList<T>` and `List<T>`, and the
`Find` method for `LinkedList<T>`:

![IndexOf operations](GRel_Test_21_IndexOf_Small.png 600)
![IndexOf operations](GRel_Test_22_IndexOf_Large.png 600)

Finding last elements
------------------------

Finding last elements uses the `LastIndexOf` method for for `ChunkedList<T>` and `List<T>`, and 
the `FindLast` method for `LinkedList<T>`:

![LastIndexOf operations](GRel_Test_23_LastIndexOf_Small.png 600)
![LastIndexOf operations](GRel_Test_24_LastIndexOf_Large.png 600)

Removing items by index
--------------------------

Removing items from their indices (`RemoveAt()` method) is not available in `LinkedList<T>`:

![RemoveAt operations](GRel_Test_25_RemoveAt_Small.png 600)
![RemoveAt operations](GRel_Test_26_RemoveAt_Large.png 600)

Inserting items by index
--------------------------

Inserting items by index (`Insert()` method) is not available in `LinkedList<T>`:

![Insert operations](GRel_Test_27_Insert_Small.png 600)
![Insert operations](GRel_Test_28_Insert_Large.png 600)

Adding a range of items
--------------------------

Adding a range of items (`AddRange()` method) is not available in `LinkedList<T>`. Both
`List<T>` and `ChunkedList<T>` structures are optimized for adding ranges originating from
arrays. A benchmark of this operation follows first:

![Adding a range of items](GRel_Test_29_AddRangeArray_Small.png 600)
![Adding a range of items](GRel_Test_30_AddRangeArray_Large.png 600)

A second benchmark is performed when the range is an enumeration not based on an array:

![Adding a range of items](GRel_Test_31_AddRangeEnumeration_Small.png 600)
![Adding a range of items](GRel_Test_32_AddRangeEnumeration_Large.png 600)
