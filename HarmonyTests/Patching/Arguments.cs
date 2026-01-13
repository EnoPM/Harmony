using HarmonyLib;
using HarmonyLibTests.Assets;
using NUnit.Framework;
using System;

namespace HarmonyLibTests.Patching
{
	[TestFixture, NonParallelizable]
	public class Arguments : TestLogger
	{
		[Test]
		public void Test_Method6()
		{
			var originalClass = typeof(Class6);
			ClassicAssert.NotNull(originalClass);
			var originalMethod = originalClass.GetMethod("Method6");
			ClassicAssert.NotNull(originalMethod);

			var patchClass = typeof(Class6Patch);
			var prefix = patchClass.GetMethod("Prefix");
			ClassicAssert.NotNull(prefix);

			var instance = new Harmony("test");
			ClassicAssert.NotNull(instance);

			var patcher = instance.CreateProcessor(originalMethod);
			ClassicAssert.NotNull(patcher);
			_ = patcher.AddPrefix(prefix);
			ClassicAssert.NotNull(patcher);

			_ = patcher.Patch();

			var instance6 = new Class6
			{
				someFloat = 999,
				someString = "original",
				someStruct = new Class6Struct() { d1 = 1, d2 = 2, d3 = 3 }
			};
			var res = instance6.Method6();
			ClassicAssert.AreEqual(res[0], 123);
			ClassicAssert.AreEqual(res[1], "patched");
			ClassicAssert.AreEqual(((Class6Struct)res[2]).d1, 10.0);
		}

		[Test]
		public void Test_Method7()
		{
			var originalClass = typeof(Class7);
			ClassicAssert.NotNull(originalClass);
			var originalMethod = originalClass.GetMethod("Method7");
			ClassicAssert.NotNull(originalMethod);

			var patchClass = typeof(Class7Patch);
			var postfix = patchClass.GetMethod("Postfix");
			ClassicAssert.NotNull(postfix);

			var instance = new Harmony("test");
			ClassicAssert.NotNull(instance);

			var patcher = instance.CreateProcessor(originalMethod);
			ClassicAssert.NotNull(patcher);
			_ = patcher.AddPostfix(postfix);

			_ = patcher.Patch();

			var instance7 = new Class7();
			var result = instance7.Method7("parameter");

			ClassicAssert.AreEqual("parameter", instance7.state1);
			ClassicAssert.AreEqual(10, result.a);
			ClassicAssert.AreEqual(20, result.b);
		}

		[Test]
		public void Test_Method8()
		{
			var originalClass = typeof(Class8);
			ClassicAssert.NotNull(originalClass);
			var originalMethod = originalClass.GetMethod("Method8");
			ClassicAssert.NotNull(originalMethod);

			var patchClass = typeof(Class8Patch);
			var postfix = patchClass.GetMethod("Postfix");
			ClassicAssert.NotNull(postfix);

			var instance = new Harmony("test");
			ClassicAssert.NotNull(instance);

			var patcher = instance.CreateProcessor(originalMethod);
			ClassicAssert.NotNull(patcher);
			_ = patcher.AddPostfix(postfix);
			ClassicAssert.NotNull(patcher);

			_ = patcher.Patch();

			var result = Class8.Method8("patched");

			ClassicAssert.True(Class8.mainRun);
			ClassicAssert.AreEqual(10, result.a);
			ClassicAssert.AreEqual(20, result.b);
		}

		[Test]
		public void Test_InjectingBaseClassField()
		{
			var testInstance = new InjectFieldSubClass();
			testInstance.Method("foo");
			ClassicAssert.AreEqual("foo", testInstance.TestValue);

			var originalClass = testInstance.GetType();
			ClassicAssert.NotNull(originalClass);
			var originalMethod = originalClass.GetMethod("Method");
			ClassicAssert.NotNull(originalMethod);

			var patchClass = typeof(InjectFieldSubClass_Patch);
			var postfix = patchClass.GetMethod("Postfix");
			ClassicAssert.NotNull(postfix);

			var instance = new Harmony("test");
			ClassicAssert.NotNull(instance);

			var patcher = instance.CreateProcessor(originalMethod);
			ClassicAssert.NotNull(patcher);
			_ = patcher.AddPostfix(postfix);
			ClassicAssert.NotNull(patcher);

			_ = patcher.Patch();

			testInstance.Method("bar");
			ClassicAssert.AreEqual("patched", testInstance.TestValue);
		}

		[Test]
		public void Test_InjectBaseDelegateForClass()
		{
			var instance = new InjectDelegateClass() { pre = "{", post = "}" };
			instance.Method(123);
			ClassicAssert.AreEqual("[{test:123}]", instance.result);

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(InjectDelegateClassPatch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			instance.Method(123);
			ClassicAssert.AreEqual("{patch:456} | [{patch:456}]", InjectDelegateClassPatch.result);
		}

		[Test]
		public void Test_InjectDelegateForStaticClass()
		{
			ClassicAssert.AreEqual("[1999]", InjectDelegateStaticClass.Method(999));

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(InjectDelegateStaticClassPatch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);
			ClassicAssert.AreEqual("[123]/[456]", InjectDelegateStaticClass.Method(4444));
		}

		[Test]
		public void Test_InjectDelegateForValueType()
		{
			var instance = new InjectDelegateStruct() { pre = "{", post = "}" };
			ClassicAssert.AreEqual("{1999}", instance.Method(999));

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(InjectDelegateStructPatch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);
			ClassicAssert.AreEqual("{123}/{456}", instance.Method(4444));
		}

		[Test]
		public void Test_RefResults()
		{
			var intRef1 = Class19.Method19();
			ClassicAssert.AreEqual("abc", intRef1);

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(Class19Patch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			var intRef2 = Class19.Method19();
			ClassicAssert.AreEqual("def", intRef2);
		}

		[Test]
		public void Test_BoxingValueResults()
		{
			var struct1 = Class20.Method20();
			ClassicAssert.AreEqual(123, struct1.value);

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(Class20Patch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			_ = Class20.Method20();
			var result = (Class20.Struct20)Class20Patch.theResult;
			ClassicAssert.AreEqual(123, result.value);
		}

		[Test]
		public void Test_BoxingRefValueResults()
		{
			var struct1 = Class21.Method21();
			ClassicAssert.AreEqual(123, struct1.value);

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(Class21Patch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			var result = Class21.Method21();
			ClassicAssert.AreEqual(456, result.value);
		}

		[Test]
		public void Test_ArgumentCases()
		{
			var harmony = new Harmony("test");
			typeof(ArgumentOriginalMethods).GetMethods().Do(original =>
			{
				var name = original.Name;
				var i = name.IndexOf("_2_");
				if (i > 0)
				{
					var typeName = name.Substring(i + 3);
					var replacementName = $"To_{typeName}";
					var replacement = typeof(ArgumentPatchMethods).GetMethod(replacementName);
					ClassicAssert.NotNull(replacement, $"replacement '{replacementName}'");
					try
					{
						var result = harmony.Patch(original, new HarmonyMethod(replacement));
						ClassicAssert.NotNull(result, "result");
					}
					catch (Exception ex)
					{
						ClassicAssert.Fail($"Patching {original.Name} failed:\n{ex}");
					}
				}
			});

			var instance = new ArgumentOriginalMethods();
			ArgumentPatchMethods.Reset();

			var obj = new ArgumentTypes.Object();
			instance.Object_2_Object(obj);
			instance.Object_2_ObjectRef(obj);
			instance.ObjectRef_2_Object(ref obj);
			instance.ObjectRef_2_ObjectRef(ref obj);

			var val = new ArgumentTypes.Value() { n = 100 };
			instance.Value_2_Value(val);
			instance.Value_2_Boxing(val);
			instance.Value_2_ValueRef(val);
			ClassicAssert.AreEqual(100, val.n);
			instance.Value_2_BoxingRef(val);
			instance.ValueRef_2_Value(ref val);
			instance.ValueRef_2_Boxing(ref val);
			instance.ValueRef_2_ValueRef(ref val);
			ClassicAssert.AreEqual(101, val.n);
			instance.ValueRef_2_BoxingRef(ref val);
			ClassicAssert.AreEqual(102, val.n);

			ClassicAssert.AreEqual("OOOOVVVVVVVV", ArgumentPatchMethods.result);
		}

		[Test]
		public void Test_SimpleArgumentArrayUsage()
		{
			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(SimpleArgumentArrayUsagePatch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			SimpleArgumentArrayUsage.n = 0;
			SimpleArgumentArrayUsage.s = "";
			SimpleArgumentArrayUsage.st = new SimpleArgumentArrayUsage.SomeStruct() { n = 0 };
			SimpleArgumentArrayUsage.f = [];

			var instance = new SimpleArgumentArrayUsage();
			instance.Method(
				100,
				"original",
				new SimpleArgumentArrayUsage.SomeStruct() { n = 200 },
				[10f, 20f, 30f]
			);

			ClassicAssert.AreEqual(123, SimpleArgumentArrayUsage.n);
			ClassicAssert.AreEqual("patched", SimpleArgumentArrayUsage.s);
			ClassicAssert.AreEqual(456, SimpleArgumentArrayUsage.st.n);
			ClassicAssert.AreEqual(3, SimpleArgumentArrayUsage.f.Length);
			ClassicAssert.AreEqual(1.2f, SimpleArgumentArrayUsage.f[0]);
			ClassicAssert.AreEqual(3.4f, SimpleArgumentArrayUsage.f[1]);
			ClassicAssert.AreEqual(5.6f, SimpleArgumentArrayUsage.f[2]);
		}

		[Test]
		public void Test_ArrayArguments()
		{
			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(ArgumentArrayPatches));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			ArgumentArrayPatches.prefixInput = null;
			ArgumentArrayPatches.postfixInput = null;

			var instance = new ArgumentArrayMethods();
			var n1 = 8;
			var n2 = 9;
			var s1 = "A";
			var s2 = "B";
			var st1 = new ArgumentArrayMethods.SomeStruct() { n = 8 };
			var st2 = new ArgumentArrayMethods.SomeStruct() { n = 9 };
			var f1 = new float[] { 8f };
			var f2 = new float[] { 9f };
			var b1 = true;
			var b2 = true;
			var e1 = ArgumentArrayMethods.ShorterThanNormal.y;
			var e2 = ArgumentArrayMethods.ShorterThanNormal.y;
			var e4 = ArgumentArrayMethods.LongerThanNormal.z;
			var e5 = ArgumentArrayMethods.LongerThanNormal.z;
			var p1 = new UIntPtr(9);
			var p2 = new UIntPtr(10);
			var m1 = (nuint)11;
			var m2 = (nuint)22;
			var d1 = new DateTime(11);
			var d2 = new DateTime(12);
			var k1 = 1111M;
			var k2 = 2222M;

			instance.Method(
				n1, ref n2, out var n3,
				s1, ref s2, out var s3,
				st1, ref st2, out var st3,
				f1, ref f2, out var f3,
				b1, ref b2, out var b3,
				e1, ref e2, out var e3,
				e4, ref e5, out var e6,
				p1, ref p2, out var p3,
				m1, ref m2, out var m3,
				d1, ref d2, out var d3,
				k1, ref k2, out var k3
			);

			// prefix input
			var r = ArgumentArrayPatches.prefixInput;
			var i = 0;
			ClassicAssert.AreEqual(8, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(9, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(0, r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual("A", r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual("B", r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(null, r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(8, ((ArgumentArrayMethods.SomeStruct)r[i]).n, $"prefix[{i++}]");
			ClassicAssert.AreEqual(9, ((ArgumentArrayMethods.SomeStruct)r[i]).n, $"prefix[{i++}]");
			ClassicAssert.AreEqual(0, ((ArgumentArrayMethods.SomeStruct)r[i]).n, $"prefix[{i++}]");

			ClassicAssert.AreEqual(8f, ((float[])r[i])[0], $"prefix[{i++}]");
			ClassicAssert.AreEqual(9f, ((float[])r[i])[0], $"prefix[{i++}]");
			ClassicAssert.AreEqual(null, (float[])r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(true, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(true, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(false, r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.y, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.y, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.a, r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.z, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.z, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.c, r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(new UIntPtr(9), r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(new UIntPtr(10), r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(new UIntPtr(0), r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual((nuint)11, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual((nuint)22, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual((nuint)0, r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(new DateTime(11),r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(new DateTime(12), r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(new DateTime(0), r[i], $"prefix[{i++}]");

			ClassicAssert.AreEqual(1111M, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(2222M, r[i], $"prefix[{i++}]");
			ClassicAssert.AreEqual(0M, r[i], $"prefix[{i++}]");

			// postfix input
			r = ArgumentArrayPatches.postfixInput;
			i = 0;
			ClassicAssert.AreEqual(8, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(123, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(456, r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual("A", r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual("abc", r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual("def", r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual(8, ((ArgumentArrayMethods.SomeStruct)r[i]).n, $"postfix[{i++}]");
			ClassicAssert.AreEqual(123, ((ArgumentArrayMethods.SomeStruct)r[i]).n, $"postfix[{i++}]");
			ClassicAssert.AreEqual(456, ((ArgumentArrayMethods.SomeStruct)r[i]).n, $"postfix[{i++}]");

			ClassicAssert.AreEqual(8f, ((float[])r[i])[0], $"postfix[{i++}]");
			ClassicAssert.AreEqual(5.6f, ((float[])r[i])[2], $"postfix[{i++}]");
			ClassicAssert.AreEqual(6.5f, ((float[])r[i])[2], $"postfix[{i++}]");

			ClassicAssert.AreEqual(true, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(false, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(true, r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.y, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.a, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.b, r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.z, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.c, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.d, r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual(new UIntPtr(9), r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(new UIntPtr(1), r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(new UIntPtr(2), r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual((nuint)11, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual((nuint)789, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual((nuint)101, r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual(new DateTime(11), r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(new DateTime(3), r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(new DateTime(4), r[i], $"postfix[{i++}]");

			ClassicAssert.AreEqual(1111M, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(666M, r[i], $"postfix[{i++}]");
			ClassicAssert.AreEqual(777M, r[i], $"postfix[{i++}]");

			// method output values
			ClassicAssert.AreEqual(123, n2, "n2");
			ClassicAssert.AreEqual(456, n3, "n3");
			ClassicAssert.AreEqual("abc", s2, "s2");
			ClassicAssert.AreEqual("def", s3, "s3");
			ClassicAssert.AreEqual(123, st2.n, "st2");
			ClassicAssert.AreEqual(456, st3.n, "st3");
			ClassicAssert.AreEqual(5.6f, f2[2], "f2");
			ClassicAssert.AreEqual(6.5f, f3[2], "f3");
			ClassicAssert.AreEqual(false, b2, $"b2");
			ClassicAssert.AreEqual(true, b3, $"b3");
			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.a, e2, $"e2");
			ClassicAssert.AreEqual(ArgumentArrayMethods.ShorterThanNormal.b, e3, $"e3");
			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.c, e5, $"e5");
			ClassicAssert.AreEqual(ArgumentArrayMethods.LongerThanNormal.d, e6, $"e6");
			ClassicAssert.AreEqual(new UIntPtr(1), p2, $"p2");
			ClassicAssert.AreEqual(new UIntPtr(2), p3, $"p3");
			ClassicAssert.AreEqual((nuint)789, m2, $"m2");
			ClassicAssert.AreEqual((nuint)101, m3, $"m3");
			ClassicAssert.AreEqual(new DateTime(3), d2, $"d2");
			ClassicAssert.AreEqual(new DateTime(4), d3, $"d3");
			ClassicAssert.AreEqual(666M, k2, $"k2");
			ClassicAssert.AreEqual(777M, k3, $"k3");
		}

		[Test]
		public void Test_RenamedArguments()
		{
			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(RenamedArgumentsPatch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);
			RenamedArgumentsPatch.log.Clear();
			new RenamedArguments().Method("test");
			var log = RenamedArgumentsPatch.log.Join();
			ClassicAssert.AreEqual("val1, patched, val2, hello", log);
		}

		[Test, Explicit("Crashes and throws NRE in some configurations: see https://discord.com/channels/131466550938042369/674571535570305060/1319451813975687269")]
		public void Test_NullableResults()
		{
			var res1 = new NullableResults().Method();
			ClassicAssert.True(res1.HasValue);
			ClassicAssert.False(res1.Value);

			var harmony = new Harmony("test");
			var processor = new PatchClassProcessor(harmony, typeof(NullableResultsPatch));
			var patches = processor.Patch();
			ClassicAssert.NotNull(patches, "patches");
			ClassicAssert.AreEqual(1, patches.Count);

			var res2 = new NullableResults().Method();
			ClassicAssert.True(res2.HasValue);
			ClassicAssert.True(res2.Value);
		}
	}
}
