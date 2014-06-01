ValueUtils
==========

A C# library to make work with value-semantics easier by e.g. (efficiently) implementing Equals and GetHashCode for you.

The library is available on nuget (for import or direct download) as [ValueUtils](https://www.nuget.org/packages/ValueUtils/)


Usage:
---

The easiest way to use value semantics is to derive from `ValueObject<>`, for example:

```C#
using ValueUtils;
sealed class MyValueObject : ValueObject<MyValueObject> {
	public int A, B, C;
	public string X,Y, Z;
// ...
}
```
A class deriving from `ValueObject<T>` implements `IEquatable<T>` and has `Equals(object)`, `Equals(T)`, `GetHashCode()` and the == and != operators implemented in terms of their fields.


Given the following example class:
```C#
class ExampleClass {
	public string myMember;
	protected readonly DateTime supports_readonly_too;
	int private_int;
// ...
}
```

Hash code usage is as follows:

```C#
using ValueUtils;

Func<ExampleClass, int> hashfunc = FieldwiseHasher<ExampleClass>.Instance;
//or directly 
int hashcode = FieldwiseHasher.Hash(my_example_object);
```

Equality usage is as follows:
```C#
using ValueUtils;

Func<ExampleClass, ExampleClass, bool> equalityComparer = FieldwiseEquality<ExampleClass>.Instance;
//or directly 
bool areEqual = FieldwiseEquality.AreEqual(my_example_object, another_example_object);
```

The above implementations can be considerably faster that the built-in `ValueType`-provided defaults for `struct`s (which use reflection every call), which is why they're a good fit to help implement `GetHashCode` and `Equals` for structs.

Limitations and gotcha's
----
`ValueObject<>` supports self-referential types (like tree structures or a singly linked list), but does not support cyclical types - such as a doubly linked list.  Whenever a cycle is encountered, the hash function and equals operations will not terminate (until the stack overflows).

Equality is implemented on a per-type basis, and that means inheritance gets confusing.  It's OK to *have* a base class (and base class fields will affect hash and equality), but if you use the base-class's equality and/or hash implementation on a subclass *instance* the code will seem to work but only consider the fields of the base class.  Best practice: don't create sub-classes that add new fields; and if you do then at least never use the base-class equality+hashcode implementations.  This is why ValueObject verifies that its subclasses must be sealed.

FieldwiseHasher and FieldwiseEquality "work" on almost all types, including types with private members in other assemblies - however, if you don't know the internals, you can't be sure what's being included in the equality computations.  In particular, if an object is lazily initialized, two semantically equivalent objects might compute as unequal simply because one is initialized and the other is not.  In practice this is rarely a problem.


Performance and hash-quality
----
All performance measurements were done on an i7-4770k at a fixed clock rate of 4.0GHz.  Loops over the dataset were repeated until 10 seconds were up, then the upper quartile average reported (this minimizes interference by other processes on my dev machine since random interference is almost always bad for performance, not good).  Some hash generators (notably `struct`) are so poor that this wasn't feasible, those timings are omitted (NaN) below.  Timings are in nanoseconds per object.

<div>
  <table>
    <thead>
      <tr>
        <th colspan="7">Complicated Case with enums, nullables, and strings</th>
      </tr>
      <tr>
        <th>Name</th>
        <th>Collisions</th>
        <th>Distinct Hashcodes</th>
        <th>.ToDictionary()</th>
        <th>.Distinct().Count()</th>
        <th>.Equals()</th>
        <th>.GetHashCode()</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>ComplicatedManual</td>
        <td>0.04%</td>
        <td>2912961 / 2914000</td>
        <td>228.6</td>
        <td>202.5</td>
        <td>7.0</td>
        <td>16.9</td>
      </tr>
      <tr>
        <td>ComplicatedValueObject</td>
        <td>0.04%</td>
        <td>2912977 / 2914000</td>
        <td>261.2</td>
        <td>241.9</td>
        <td>21.4</td>
        <td>42.0</td>
      </tr>
      <tr>
        <td>Tuple&lt;SeekOrigin, int, int?, int, string, DateTime, int&gt;</td>
        <td>0.03%</td>
        <td>2913001 / 2914000</td>
        <td>492.9</td>
        <td>464.3</td>
        <td>249.4</td>
        <td>262.8</td>
      </tr>
      <tr>
        <td>ComplicatedStruct</td>
        <td>100.00%</td>
        <td>2 / 2914000</td>
        <td>NaN</td>
        <td>NaN</td>
        <td>1004.8</td>
        <td>94.2</td>
      </tr>
      <tr>
        <td>&lt;&gt;f__AnonymousTypea&lt;SeekOrigin, int, int?, int, string, DateTime, int&gt;</td>
        <td>0.03%</td>
        <td>2913022 / 2914000</td>
        <td>269.3</td>
        <td>243.1</td>
        <td>31.5</td>
        <td>52.8</td>
      </tr>
    </tbody>
  </table>
  <table>
    <thead>
      <tr>
        <th colspan="7">A simple pair of ints</th>
      </tr>
      <tr>
        <th>Name</th>
        <th>Collisions</th>
        <th>Distinct Hashcodes</th>
        <th>.ToDictionary()</th>
        <th>.Distinct().Count()</th>
        <th>.Equals()</th>
        <th>.GetHashCode()</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>IntPairManual</td>
        <td>0.02%</td>
        <td>2975318 / 2976000</td>
        <td>160.6</td>
        <td>134.9</td>
        <td>3.8</td>
        <td>1.7</td>
      </tr>
      <tr>
        <td>IntPairValueObject</td>
        <td>0.03%</td>
        <td>2974963 / 2976000</td>
        <td>202.4</td>
        <td>176.6</td>
        <td>18.8</td>
        <td>16.3</td>
      </tr>
      <tr>
        <td>Tuple&lt;int, int&gt;</td>
        <td>38.31%</td>
        <td>1835788 / 2976000</td>
        <td>322.1</td>
        <td>302.1</td>
        <td>97.6</td>
        <td>54.8</td>
      </tr>
      <tr>
        <td>IntPairStruct</td>
        <td>56.61%</td>
        <td>1291168 / 2976000</td>
        <td>856.8</td>
        <td>810.3</td>
        <td>31.4</td>
        <td>37.1</td>
      </tr>
      <tr>
        <td>&lt;&gt;f__AnonymousType5&lt;int, int&gt;</td>
        <td>4.69%</td>
        <td>2836344 / 2976000</td>
        <td>182.0</td>
        <td>153.6</td>
        <td>15.2</td>
        <td>13.5</td>
      </tr>
    </tbody>
  </table>
  <table>
    <thead>
      <tr>
        <th colspan="7">int-pair with both ints having the same value</th>
      </tr>
      <tr>
        <th>Name</th>
        <th>Collisions</th>
        <th>Distinct Hashcodes</th>
        <th>.ToDictionary()</th>
        <th>.Distinct().Count()</th>
        <th>.Equals()</th>
        <th>.GetHashCode()</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>IntPairManual</td>
        <td>0.37%</td>
        <td>2988915 / 3000000</td>
        <td>184.6</td>
        <td>152.1</td>
        <td>3.6</td>
        <td>1.7</td>
      </tr>
      <tr>
        <td>IntPairValueObject</td>
        <td>0.03%</td>
        <td>2999012 / 3000000</td>
        <td>200.0</td>
        <td>175.3</td>
        <td>18.7</td>
        <td>16.4</td>
      </tr>
      <tr>
        <td>Tuple&lt;int, int&gt;</td>
        <td>22.07%</td>
        <td>2337827 / 3000000</td>
        <td>149.9</td>
        <td>142.3</td>
        <td>77.0</td>
        <td>55.1</td>
      </tr>
      <tr>
        <td>IntPairStruct</td>
        <td>100.00%</td>
        <td>1 / 3000000</td>
        <td>NaN</td>
        <td>NaN</td>
        <td>31.9</td>
        <td>37.6</td>
      </tr>
      <tr>
        <td>&lt;&gt;f__AnonymousType5&lt;int, int&gt;</td>
        <td>0.00%</td>
        <td>3000000 / 3000000</td>
        <td>157.0</td>
        <td>108.8</td>
        <td>12.1</td>
        <td>13.5</td>
      </tr>
    </tbody>
  </table>
  <table>
    <thead>
      <tr>
        <th colspan="7">int-pair with a dataset in which both the object and it's mirror image are present</th>
      </tr>
      <tr>
        <th>Name</th>
        <th>Collisions</th>
        <th>Distinct Hashcodes</th>
        <th>.ToDictionary()</th>
        <th>.Distinct().Count()</th>
        <th>.Equals()</th>
        <th>.GetHashCode()</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>IntPairManual</td>
        <td>0.62%</td>
        <td>3014881 / 3033584</td>
        <td>164.4</td>
        <td>146.5</td>
        <td>3.6</td>
        <td>1.7</td>
      </tr>
      <tr>
        <td>IntPairValueObject</td>
        <td>0.03%</td>
        <td>3032561 / 3033584</td>
        <td>200.3</td>
        <td>177.4</td>
        <td>18.7</td>
        <td>16.4</td>
      </tr>
      <tr>
        <td>Tuple&lt;int, int&gt;</td>
        <td>41.47%</td>
        <td>1775545 / 3033584</td>
        <td>507.5</td>
        <td>456.7</td>
        <td>76.9</td>
        <td>55.0</td>
      </tr>
      <tr>
        <td>IntPairStruct</td>
        <td>74.50%</td>
        <td>773500 / 3033584</td>
        <td>809.0</td>
        <td>792.7</td>
        <td>30.9</td>
        <td>37.2</td>
      </tr>
      <tr>
        <td>&lt;&gt;f__AnonymousType5&lt;int, int&gt;</td>
        <td>0.79%</td>
        <td>3009536 / 3033584</td>
        <td>179.0</td>
        <td>165.3</td>
        <td>12.2</td>
        <td>13.5</td>
      </tr>
    </tbody>
  </table>
  <table>
    <thead>
      <tr>
        <th colspan="7">int-pair with self-reference; data set contains one level of nesting with nested values being mirror images of their containers</th>
      </tr>
      <tr>
        <th>Name</th>
        <th>Collisions</th>
        <th>Distinct Hashcodes</th>
        <th>.ToDictionary()</th>
        <th>.Distinct().Count()</th>
        <th>.Equals()</th>
        <th>.GetHashCode()</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>NastyNestedManual</td>
        <td>24.14%</td>
        <td>2267216 / 2988648</td>
        <td>371.6</td>
        <td>187.3</td>
        <td>6.3</td>
        <td>5.0</td>
      </tr>
      <tr>
        <td>NastyNestedValueObject</td>
        <td>0.03%</td>
        <td>2987634 / 2988648</td>
        <td>248.6</td>
        <td>220.4</td>
        <td>31.0</td>
        <td>32.9</td>
      </tr>
      <tr>
        <td>Tuple&lt;object, int, int&gt;</td>
        <td>57.80%</td>
        <td>1261193 / 2988648</td>
        <td>488.3</td>
        <td>492.4</td>
        <td>103.0</td>
        <td>131.1</td>
      </tr>
    </tbody>
  </table>
</div>