
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Mavri.Utils;

namespace SmartHomeDotNet.Utils
{
	public static class ObservableEx
	{
		public struct CombineTuple<T0>
		{
			private T0 _item0;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, out CombineTuple<T0> result)
			{
				result = new CombineTuple<T0>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			public void Deconstruct(out T0 item0)
			{
				
				item0 = _item0;
			}
		}

		/// <summary>
		/// Combines the latest values of 2 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		public static IObservable<CombineTuple<T0>> CombineLatest<T0>(IObservable<T0> obs0)
		{
			return Observable.Create<CombineTuple<T0>>(observer =>
			{
				var subscriptions = new CompositeDisposable(2);
				var nextGate = new object();
				var running = 2;
				var value0 = default(Option<T0>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0>.TryCreate(changedIndex, changedSource, value0, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0>
		{
			private T0 _item0;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, out IdentifiedCombineTuple<TSourceId, T0> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			public void Deconstruct(out T0 item0)
			{
				
				item0 = _item0;
			}
		}

		/// <summary>
		/// Combines the latest values of 2 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0>> CombineLatest<TSourceId, T0>(
			(TSourceId id, IObservable<T0> observable) src0)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0>>(observer =>
			{
				var subscriptions = new CompositeDisposable(2);
				var nextGate = new object();
				var running = 2;
				var value0 = default(Option<T0>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0>.TryCreate(changedId, changedIndex, changedSource, value0, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1>
		{
			private T0 _item0;
private T1 _item1;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, out CombineTuple<T0, T1> result)
			{
				result = new CombineTuple<T0, T1>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			public void Deconstruct(out T0 item0,out T1 item1)
			{
				
				item0 = _item0;
				item1 = _item1;
			}
		}

		/// <summary>
		/// Combines the latest values of 3 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		public static IObservable<CombineTuple<T0, T1>> CombineLatest<T0, T1>(IObservable<T0> obs0, IObservable<T1> obs1)
		{
			return Observable.Create<CombineTuple<T0, T1>>(observer =>
			{
				var subscriptions = new CompositeDisposable(3);
				var nextGate = new object();
				var running = 3;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1>.TryCreate(changedIndex, changedSource, value0, value1, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1>
		{
			private T0 _item0;
private T1 _item1;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, out IdentifiedCombineTuple<TSourceId, T0, T1> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			public void Deconstruct(out T0 item0,out T1 item1)
			{
				
				item0 = _item0;
				item1 = _item1;
			}
		}

		/// <summary>
		/// Combines the latest values of 3 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1>> CombineLatest<TSourceId, T0, T1>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1>>(observer =>
			{
				var subscriptions = new CompositeDisposable(3);
				var nextGate = new object();
				var running = 3;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1>.TryCreate(changedId, changedIndex, changedSource, value0, value1, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, out CombineTuple<T0, T1, T2> result)
			{
				result = new CombineTuple<T0, T1, T2>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
			}
		}

		/// <summary>
		/// Combines the latest values of 4 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		public static IObservable<CombineTuple<T0, T1, T2>> CombineLatest<T0, T1, T2>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2)
		{
			return Observable.Create<CombineTuple<T0, T1, T2>>(observer =>
			{
				var subscriptions = new CompositeDisposable(4);
				var nextGate = new object();
				var running = 4;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2>.TryCreate(changedIndex, changedSource, value0, value1, value2, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, out IdentifiedCombineTuple<TSourceId, T0, T1, T2> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
			}
		}

		/// <summary>
		/// Combines the latest values of 4 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2>> CombineLatest<TSourceId, T0, T1, T2>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2>>(observer =>
			{
				var subscriptions = new CompositeDisposable(4);
				var nextGate = new object();
				var running = 4;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, out CombineTuple<T0, T1, T2, T3> result)
			{
				result = new CombineTuple<T0, T1, T2, T3>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
			}
		}

		/// <summary>
		/// Combines the latest values of 5 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3>> CombineLatest<T0, T1, T2, T3>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3>>(observer =>
			{
				var subscriptions = new CompositeDisposable(5);
				var nextGate = new object();
				var running = 5;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
			}
		}

		/// <summary>
		/// Combines the latest values of 5 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3>> CombineLatest<TSourceId, T0, T1, T2, T3>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3>>(observer =>
			{
				var subscriptions = new CompositeDisposable(5);
				var nextGate = new object();
				var running = 5;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, out CombineTuple<T0, T1, T2, T3, T4> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
			}
		}

		/// <summary>
		/// Combines the latest values of 6 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4>> CombineLatest<T0, T1, T2, T3, T4>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4>>(observer =>
			{
				var subscriptions = new CompositeDisposable(6);
				var nextGate = new object();
				var running = 6;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
			}
		}

		/// <summary>
		/// Combines the latest values of 6 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4>> CombineLatest<TSourceId, T0, T1, T2, T3, T4>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4>>(observer =>
			{
				var subscriptions = new CompositeDisposable(6);
				var nextGate = new object();
				var running = 6;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, out CombineTuple<T0, T1, T2, T3, T4, T5> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
			}
		}

		/// <summary>
		/// Combines the latest values of 7 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5>> CombineLatest<T0, T1, T2, T3, T4, T5>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5>>(observer =>
			{
				var subscriptions = new CompositeDisposable(7);
				var nextGate = new object();
				var running = 7;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
			}
		}

		/// <summary>
		/// Combines the latest values of 7 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5>>(observer =>
			{
				var subscriptions = new CompositeDisposable(7);
				var nextGate = new object();
				var running = 7;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, out CombineTuple<T0, T1, T2, T3, T4, T5, T6> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
			}
		}

		/// <summary>
		/// Combines the latest values of 8 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6>> CombineLatest<T0, T1, T2, T3, T4, T5, T6>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6>>(observer =>
			{
				var subscriptions = new CompositeDisposable(8);
				var nextGate = new object();
				var running = 8;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
			}
		}

		/// <summary>
		/// Combines the latest values of 8 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6>>(observer =>
			{
				var subscriptions = new CompositeDisposable(8);
				var nextGate = new object();
				var running = 8;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
			}
		}

		/// <summary>
		/// Combines the latest values of 9 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7>>(observer =>
			{
				var subscriptions = new CompositeDisposable(9);
				var nextGate = new object();
				var running = 9;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
			}
		}

		/// <summary>
		/// Combines the latest values of 9 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7>>(observer =>
			{
				var subscriptions = new CompositeDisposable(9);
				var nextGate = new object();
				var running = 9;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
			}
		}

		/// <summary>
		/// Combines the latest values of 10 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		/// <param name="obs8">The observable sequence 8</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7, IObservable<T8> obs8)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8>>(observer =>
			{
				var subscriptions = new CompositeDisposable(10);
				var nextGate = new object();
				var running = 10;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs8
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(8, obs8);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
			}
		}

		/// <summary>
		/// Combines the latest values of 10 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		/// <param name="src8">The observable sequence 8</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7, 
			(TSourceId id, IObservable<T8> observable) src8)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8>>(observer =>
			{
				var subscriptions = new CompositeDisposable(10);
				var nextGate = new object();
				var running = 10;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src8
					.observable
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(src8.id, 8, src8.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
			}
		}

		/// <summary>
		/// Combines the latest values of 11 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		/// <param name="obs8">The observable sequence 8</param>
		/// <param name="obs9">The observable sequence 9</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7, IObservable<T8> obs8, IObservable<T9> obs9)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>(observer =>
			{
				var subscriptions = new CompositeDisposable(11);
				var nextGate = new object();
				var running = 11;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs8
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(8, obs8);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs9
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(9, obs9);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
			}
		}

		/// <summary>
		/// Combines the latest values of 11 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		/// <param name="src8">The observable sequence 8</param>
		/// <param name="src9">The observable sequence 9</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7, 
			(TSourceId id, IObservable<T8> observable) src8, 
			(TSourceId id, IObservable<T9> observable) src9)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>(observer =>
			{
				var subscriptions = new CompositeDisposable(11);
				var nextGate = new object();
				var running = 11;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src8
					.observable
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(src8.id, 8, src8.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src9
					.observable
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(src9.id, 9, src9.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
			}
		}

		/// <summary>
		/// Combines the latest values of 12 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		/// <param name="obs8">The observable sequence 8</param>
		/// <param name="obs9">The observable sequence 9</param>
		/// <param name="obs10">The observable sequence 10</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7, IObservable<T8> obs8, IObservable<T9> obs9, IObservable<T10> obs10)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(observer =>
			{
				var subscriptions = new CompositeDisposable(12);
				var nextGate = new object();
				var running = 12;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs8
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(8, obs8);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs9
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(9, obs9);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs10
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(10, obs10);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
			}
		}

		/// <summary>
		/// Combines the latest values of 12 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		/// <param name="src8">The observable sequence 8</param>
		/// <param name="src9">The observable sequence 9</param>
		/// <param name="src10">The observable sequence 10</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7, 
			(TSourceId id, IObservable<T8> observable) src8, 
			(TSourceId id, IObservable<T9> observable) src9, 
			(TSourceId id, IObservable<T10> observable) src10)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(observer =>
			{
				var subscriptions = new CompositeDisposable(12);
				var nextGate = new object();
				var running = 12;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src8
					.observable
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(src8.id, 8, src8.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src9
					.observable
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(src9.id, 9, src9.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src10
					.observable
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(src10.id, 10, src10.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;
private T11 _item11;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, Option<T11> value11, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10) && 
					value11.MatchSome(out result._item11);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;
case 11: return _item11;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// The last value produced the 11 observable sequence
			/// </summary>
			public T11 Item11 => _item11;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			/// <param name="item11">Output variable 11</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10,out T11 item11)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
				item11 = _item11;
			}
		}

		/// <summary>
		/// Combines the latest values of 13 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		/// <param name="obs8">The observable sequence 8</param>
		/// <param name="obs9">The observable sequence 9</param>
		/// <param name="obs10">The observable sequence 10</param>
		/// <param name="obs11">The observable sequence 11</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7, IObservable<T8> obs8, IObservable<T9> obs9, IObservable<T10> obs10, IObservable<T11> obs11)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(observer =>
			{
				var subscriptions = new CompositeDisposable(13);
				var nextGate = new object();
				var running = 13;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);
var value11 = default(Option<T11>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs8
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(8, obs8);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs9
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(9, obs9);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs10
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(10, obs10);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs11
					.Subscribe(
						v11 =>
						{
							value11 = v11;
							OnNext(11, obs11);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;
private T11 _item11;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, Option<T11> value11, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10) && 
					value11.MatchSome(out result._item11);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;
case 11: return _item11;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// The last value produced the 11 observable sequence
			/// </summary>
			public T11 Item11 => _item11;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			/// <param name="item11">Output variable 11</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10,out T11 item11)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
				item11 = _item11;
			}
		}

		/// <summary>
		/// Combines the latest values of 13 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		/// <param name="src8">The observable sequence 8</param>
		/// <param name="src9">The observable sequence 9</param>
		/// <param name="src10">The observable sequence 10</param>
		/// <param name="src11">The observable sequence 11</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7, 
			(TSourceId id, IObservable<T8> observable) src8, 
			(TSourceId id, IObservable<T9> observable) src9, 
			(TSourceId id, IObservable<T10> observable) src10, 
			(TSourceId id, IObservable<T11> observable) src11)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(observer =>
			{
				var subscriptions = new CompositeDisposable(13);
				var nextGate = new object();
				var running = 13;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);
var value11 = default(Option<T11>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src8
					.observable
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(src8.id, 8, src8.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src9
					.observable
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(src9.id, 9, src9.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src10
					.observable
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(src10.id, 10, src10.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src11
					.observable
					.Subscribe(
						v11 =>
						{
							value11 = v11;
							OnNext(src11.id, 11, src11.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;
private T11 _item11;
private T12 _item12;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, Option<T11> value11, Option<T12> value12, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10) && 
					value11.MatchSome(out result._item11) && 
					value12.MatchSome(out result._item12);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;
case 11: return _item11;
case 12: return _item12;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// The last value produced the 11 observable sequence
			/// </summary>
			public T11 Item11 => _item11;

			/// <summary>
			/// The last value produced the 12 observable sequence
			/// </summary>
			public T12 Item12 => _item12;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			/// <param name="item11">Output variable 11</param>
			/// <param name="item12">Output variable 12</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10,out T11 item11,out T12 item12)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
				item11 = _item11;
				item12 = _item12;
			}
		}

		/// <summary>
		/// Combines the latest values of 14 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		/// <param name="obs8">The observable sequence 8</param>
		/// <param name="obs9">The observable sequence 9</param>
		/// <param name="obs10">The observable sequence 10</param>
		/// <param name="obs11">The observable sequence 11</param>
		/// <param name="obs12">The observable sequence 12</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7, IObservable<T8> obs8, IObservable<T9> obs9, IObservable<T10> obs10, IObservable<T11> obs11, IObservable<T12> obs12)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(observer =>
			{
				var subscriptions = new CompositeDisposable(14);
				var nextGate = new object();
				var running = 14;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);
var value11 = default(Option<T11>);
var value12 = default(Option<T12>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs8
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(8, obs8);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs9
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(9, obs9);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs10
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(10, obs10);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs11
					.Subscribe(
						v11 =>
						{
							value11 = v11;
							OnNext(11, obs11);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs12
					.Subscribe(
						v12 =>
						{
							value12 = v12;
							OnNext(12, obs12);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;
private T11 _item11;
private T12 _item12;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, Option<T11> value11, Option<T12> value12, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10) && 
					value11.MatchSome(out result._item11) && 
					value12.MatchSome(out result._item12);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;
case 11: return _item11;
case 12: return _item12;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// The last value produced the 11 observable sequence
			/// </summary>
			public T11 Item11 => _item11;

			/// <summary>
			/// The last value produced the 12 observable sequence
			/// </summary>
			public T12 Item12 => _item12;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			/// <param name="item11">Output variable 11</param>
			/// <param name="item12">Output variable 12</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10,out T11 item11,out T12 item12)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
				item11 = _item11;
				item12 = _item12;
			}
		}

		/// <summary>
		/// Combines the latest values of 14 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		/// <param name="src8">The observable sequence 8</param>
		/// <param name="src9">The observable sequence 9</param>
		/// <param name="src10">The observable sequence 10</param>
		/// <param name="src11">The observable sequence 11</param>
		/// <param name="src12">The observable sequence 12</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7, 
			(TSourceId id, IObservable<T8> observable) src8, 
			(TSourceId id, IObservable<T9> observable) src9, 
			(TSourceId id, IObservable<T10> observable) src10, 
			(TSourceId id, IObservable<T11> observable) src11, 
			(TSourceId id, IObservable<T12> observable) src12)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(observer =>
			{
				var subscriptions = new CompositeDisposable(14);
				var nextGate = new object();
				var running = 14;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);
var value11 = default(Option<T11>);
var value12 = default(Option<T12>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src8
					.observable
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(src8.id, 8, src8.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src9
					.observable
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(src9.id, 9, src9.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src10
					.observable
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(src10.id, 10, src10.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src11
					.observable
					.Subscribe(
						v11 =>
						{
							value11 = v11;
							OnNext(src11.id, 11, src11.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src12
					.observable
					.Subscribe(
						v12 =>
						{
							value12 = v12;
							OnNext(src12.id, 12, src12.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


		public struct CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;
private T11 _item11;
private T12 _item12;
private T13 _item13;

			internal static bool TryCreate(int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, Option<T11> value11, Option<T12> value12, Option<T13> value13, out CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> result)
			{
				result = new CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10) && 
					value11.MatchSome(out result._item11) && 
					value12.MatchSome(out result._item12) && 
					value13.MatchSome(out result._item13);
			}

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;
case 11: return _item11;
case 12: return _item12;
case 13: return _item13;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// The last value produced the 11 observable sequence
			/// </summary>
			public T11 Item11 => _item11;

			/// <summary>
			/// The last value produced the 12 observable sequence
			/// </summary>
			public T12 Item12 => _item12;

			/// <summary>
			/// The last value produced the 13 observable sequence
			/// </summary>
			public T13 Item13 => _item13;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			/// <param name="item11">Output variable 11</param>
			/// <param name="item12">Output variable 12</param>
			/// <param name="item13">Output variable 13</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10,out T11 item11,out T12 item12,out T13 item13)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
				item11 = _item11;
				item12 = _item12;
				item13 = _item13;
			}
		}

		/// <summary>
		/// Combines the latest values of 15 observables sequences
		/// </summary>
		
		/// <param name="obs0">The observable sequence 0</param>
		/// <param name="obs1">The observable sequence 1</param>
		/// <param name="obs2">The observable sequence 2</param>
		/// <param name="obs3">The observable sequence 3</param>
		/// <param name="obs4">The observable sequence 4</param>
		/// <param name="obs5">The observable sequence 5</param>
		/// <param name="obs6">The observable sequence 6</param>
		/// <param name="obs7">The observable sequence 7</param>
		/// <param name="obs8">The observable sequence 8</param>
		/// <param name="obs9">The observable sequence 9</param>
		/// <param name="obs10">The observable sequence 10</param>
		/// <param name="obs11">The observable sequence 11</param>
		/// <param name="obs12">The observable sequence 12</param>
		/// <param name="obs13">The observable sequence 13</param>
		public static IObservable<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> CombineLatest<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(IObservable<T0> obs0, IObservable<T1> obs1, IObservable<T2> obs2, IObservable<T3> obs3, IObservable<T4> obs4, IObservable<T5> obs5, IObservable<T6> obs6, IObservable<T7> obs7, IObservable<T8> obs8, IObservable<T9> obs9, IObservable<T10> obs10, IObservable<T11> obs11, IObservable<T12> obs12, IObservable<T13> obs13)
		{
			return Observable.Create<CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(observer =>
			{
				var subscriptions = new CompositeDisposable(15);
				var nextGate = new object();
				var running = 15;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);
var value11 = default(Option<T11>);
var value12 = default(Option<T12>);
var value13 = default(Option<T13>);;

				
				obs0
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(0, obs0);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs1
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(1, obs1);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs2
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(2, obs2);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs3
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(3, obs3);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs4
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(4, obs4);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs5
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(5, obs5);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs6
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(6, obs6);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs7
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(7, obs7);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs8
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(8, obs8);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs9
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(9, obs9);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs10
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(10, obs10);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs11
					.Subscribe(
						v11 =>
						{
							value11 = v11;
							OnNext(11, obs11);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs12
					.Subscribe(
						v12 =>
						{
							value12 = v12;
							OnNext(12, obs12);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				obs13
					.Subscribe(
						v13 =>
						{
							value13 = v13;
							OnNext(13, obs13);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(int changedIndex, object changedSource)
				{
					if (CombineTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.TryCreate(changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}

		public struct IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
		{
			private T0 _item0;
private T1 _item1;
private T2 _item2;
private T3 _item3;
private T4 _item4;
private T5 _item5;
private T6 _item6;
private T7 _item7;
private T8 _item8;
private T9 _item9;
private T10 _item10;
private T11 _item11;
private T12 _item12;
private T13 _item13;

			internal static bool TryCreate(TSourceId changedId, int changedIndex, object changedSource, Option<T0> value0, Option<T1> value1, Option<T2> value2, Option<T3> value3, Option<T4> value4, Option<T5> value5, Option<T6> value6, Option<T7> value7, Option<T8> value8, Option<T9> value9, Option<T10> value10, Option<T11> value11, Option<T12> value12, Option<T13> value13, out IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> result)
			{
				result = new IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
				result.ChangedId = changedId;
				result.ChangedIndex = changedIndex;
				result.ChangedSource = changedSource;

				return 
					value0.MatchSome(out result._item0) && 
					value1.MatchSome(out result._item1) && 
					value2.MatchSome(out result._item2) && 
					value3.MatchSome(out result._item3) && 
					value4.MatchSome(out result._item4) && 
					value5.MatchSome(out result._item5) && 
					value6.MatchSome(out result._item6) && 
					value7.MatchSome(out result._item7) && 
					value8.MatchSome(out result._item8) && 
					value9.MatchSome(out result._item9) && 
					value10.MatchSome(out result._item10) && 
					value11.MatchSome(out result._item11) && 
					value12.MatchSome(out result._item12) && 
					value13.MatchSome(out result._item13);
			}

			/// <summary>
			/// Gets identifier of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public TSourceId ChangedId { get; private set; }

			/// <summary>
			/// Gets the 0-based index of the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public int ChangedIndex { get; private set; }

			/// <summary>
			/// The source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedSource { get; private set; }

			/// <summary>
			/// The last value poduced by the source observable sequence which caused this value to be published in the result observable sequence
			/// </summary>
			public object ChangedValue
			{
				get
				{
					switch(ChangedIndex)
					{
						case 0: return _item0;
case 1: return _item1;
case 2: return _item2;
case 3: return _item3;
case 4: return _item4;
case 5: return _item5;
case 6: return _item6;
case 7: return _item7;
case 8: return _item8;
case 9: return _item9;
case 10: return _item10;
case 11: return _item11;
case 12: return _item12;
case 13: return _item13;

						default:
							throw new ArgumentOutOfRangeException(nameof(ChangedIndex));
					}
				}
			}

			
			/// <summary>
			/// The last value produced the 0 observable sequence
			/// </summary>
			public T0 Item0 => _item0;

			/// <summary>
			/// The last value produced the 1 observable sequence
			/// </summary>
			public T1 Item1 => _item1;

			/// <summary>
			/// The last value produced the 2 observable sequence
			/// </summary>
			public T2 Item2 => _item2;

			/// <summary>
			/// The last value produced the 3 observable sequence
			/// </summary>
			public T3 Item3 => _item3;

			/// <summary>
			/// The last value produced the 4 observable sequence
			/// </summary>
			public T4 Item4 => _item4;

			/// <summary>
			/// The last value produced the 5 observable sequence
			/// </summary>
			public T5 Item5 => _item5;

			/// <summary>
			/// The last value produced the 6 observable sequence
			/// </summary>
			public T6 Item6 => _item6;

			/// <summary>
			/// The last value produced the 7 observable sequence
			/// </summary>
			public T7 Item7 => _item7;

			/// <summary>
			/// The last value produced the 8 observable sequence
			/// </summary>
			public T8 Item8 => _item8;

			/// <summary>
			/// The last value produced the 9 observable sequence
			/// </summary>
			public T9 Item9 => _item9;

			/// <summary>
			/// The last value produced the 10 observable sequence
			/// </summary>
			public T10 Item10 => _item10;

			/// <summary>
			/// The last value produced the 11 observable sequence
			/// </summary>
			public T11 Item11 => _item11;

			/// <summary>
			/// The last value produced the 12 observable sequence
			/// </summary>
			public T12 Item12 => _item12;

			/// <summary>
			/// The last value produced the 13 observable sequence
			/// </summary>
			public T13 Item13 => _item13;

			/// <summary>
			/// Deconstructs this tuple into multiple variables
			/// </summary>
			
			/// <param name="item0">Output variable 0</param>
			/// <param name="item1">Output variable 1</param>
			/// <param name="item2">Output variable 2</param>
			/// <param name="item3">Output variable 3</param>
			/// <param name="item4">Output variable 4</param>
			/// <param name="item5">Output variable 5</param>
			/// <param name="item6">Output variable 6</param>
			/// <param name="item7">Output variable 7</param>
			/// <param name="item8">Output variable 8</param>
			/// <param name="item9">Output variable 9</param>
			/// <param name="item10">Output variable 10</param>
			/// <param name="item11">Output variable 11</param>
			/// <param name="item12">Output variable 12</param>
			/// <param name="item13">Output variable 13</param>
			public void Deconstruct(out T0 item0,out T1 item1,out T2 item2,out T3 item3,out T4 item4,out T5 item5,out T6 item6,out T7 item7,out T8 item8,out T9 item9,out T10 item10,out T11 item11,out T12 item12,out T13 item13)
			{
				
				item0 = _item0;
				item1 = _item1;
				item2 = _item2;
				item3 = _item3;
				item4 = _item4;
				item5 = _item5;
				item6 = _item6;
				item7 = _item7;
				item8 = _item8;
				item9 = _item9;
				item10 = _item10;
				item11 = _item11;
				item12 = _item12;
				item13 = _item13;
			}
		}

		/// <summary>
		/// Combines the latest values of 15 observables sequences
		/// </summary>
		
		/// <param name="src0">The observable sequence 0</param>
		/// <param name="src1">The observable sequence 1</param>
		/// <param name="src2">The observable sequence 2</param>
		/// <param name="src3">The observable sequence 3</param>
		/// <param name="src4">The observable sequence 4</param>
		/// <param name="src5">The observable sequence 5</param>
		/// <param name="src6">The observable sequence 6</param>
		/// <param name="src7">The observable sequence 7</param>
		/// <param name="src8">The observable sequence 8</param>
		/// <param name="src9">The observable sequence 9</param>
		/// <param name="src10">The observable sequence 10</param>
		/// <param name="src11">The observable sequence 11</param>
		/// <param name="src12">The observable sequence 12</param>
		/// <param name="src13">The observable sequence 13</param>
		public static IObservable<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> CombineLatest<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
			(TSourceId id, IObservable<T0> observable) src0, 
			(TSourceId id, IObservable<T1> observable) src1, 
			(TSourceId id, IObservable<T2> observable) src2, 
			(TSourceId id, IObservable<T3> observable) src3, 
			(TSourceId id, IObservable<T4> observable) src4, 
			(TSourceId id, IObservable<T5> observable) src5, 
			(TSourceId id, IObservable<T6> observable) src6, 
			(TSourceId id, IObservable<T7> observable) src7, 
			(TSourceId id, IObservable<T8> observable) src8, 
			(TSourceId id, IObservable<T9> observable) src9, 
			(TSourceId id, IObservable<T10> observable) src10, 
			(TSourceId id, IObservable<T11> observable) src11, 
			(TSourceId id, IObservable<T12> observable) src12, 
			(TSourceId id, IObservable<T13> observable) src13)
		{
			return Observable.Create<IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(observer =>
			{
				var subscriptions = new CompositeDisposable(15);
				var nextGate = new object();
				var running = 15;
				var value0 = default(Option<T0>);
var value1 = default(Option<T1>);
var value2 = default(Option<T2>);
var value3 = default(Option<T3>);
var value4 = default(Option<T4>);
var value5 = default(Option<T5>);
var value6 = default(Option<T6>);
var value7 = default(Option<T7>);
var value8 = default(Option<T8>);
var value9 = default(Option<T9>);
var value10 = default(Option<T10>);
var value11 = default(Option<T11>);
var value12 = default(Option<T12>);
var value13 = default(Option<T13>);;

				
				src0
					.observable
					.Subscribe(
						v0 =>
						{
							value0 = v0;
							OnNext(src0.id, 0, src0.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src1
					.observable
					.Subscribe(
						v1 =>
						{
							value1 = v1;
							OnNext(src1.id, 1, src1.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src2
					.observable
					.Subscribe(
						v2 =>
						{
							value2 = v2;
							OnNext(src2.id, 2, src2.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src3
					.observable
					.Subscribe(
						v3 =>
						{
							value3 = v3;
							OnNext(src3.id, 3, src3.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src4
					.observable
					.Subscribe(
						v4 =>
						{
							value4 = v4;
							OnNext(src4.id, 4, src4.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src5
					.observable
					.Subscribe(
						v5 =>
						{
							value5 = v5;
							OnNext(src5.id, 5, src5.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src6
					.observable
					.Subscribe(
						v6 =>
						{
							value6 = v6;
							OnNext(src6.id, 6, src6.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src7
					.observable
					.Subscribe(
						v7 =>
						{
							value7 = v7;
							OnNext(src7.id, 7, src7.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src8
					.observable
					.Subscribe(
						v8 =>
						{
							value8 = v8;
							OnNext(src8.id, 8, src8.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src9
					.observable
					.Subscribe(
						v9 =>
						{
							value9 = v9;
							OnNext(src9.id, 9, src9.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src10
					.observable
					.Subscribe(
						v10 =>
						{
							value10 = v10;
							OnNext(src10.id, 10, src10.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src11
					.observable
					.Subscribe(
						v11 =>
						{
							value11 = v11;
							OnNext(src11.id, 11, src11.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src12
					.observable
					.Subscribe(
						v12 =>
						{
							value12 = v12;
							OnNext(src12.id, 12, src12.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				src13
					.observable
					.Subscribe(
						v13 =>
						{
							value13 = v13;
							OnNext(src13.id, 13, src13.observable);
						},
						observer.OnError,
						OnComplete)
					.DisposeWith(subscriptions);
				

				return subscriptions;

				void OnNext(TSourceId changedId, int changedIndex, object changedSource)
				{
					if (IdentifiedCombineTuple<TSourceId, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.TryCreate(changedId, changedIndex, changedSource, value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, out var tuple))
					{
						lock (nextGate)
						{
							observer.OnNext(tuple);
						}
					}
				}

				void OnComplete()
				{
					if (Interlocked.Decrement(ref running) <= 0)
					{
						observer.OnCompleted();
					}
				}
			});
		}


	}
}
