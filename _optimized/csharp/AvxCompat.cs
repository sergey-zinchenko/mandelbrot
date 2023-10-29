/*
 * Copyright (C) 2023. Sergei Zinchenko. All rights reserved.

 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.

 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace ConsoleApp;

public class AvxCompat
{
    protected AvxCompat()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<Single> Multiply(Vector256<Single> a, Vector256<Single> b)
    {
        return Vector256.Create(AdvSimd.Multiply(a.GetLower(), b.GetLower()),
            AdvSimd.Multiply(a.GetUpper(), b.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<Single> Add(Vector256<Single> a, Vector256<Single> b)
    {
        return Vector256.Create(AdvSimd.Add(a.GetLower(), b.GetLower()),
            AdvSimd.Add(a.GetUpper(), b.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_unord_q_ps(Vector128<Single> a, Vector128<Single> b)
    {
        Vector128<uint> result;
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;
            ptrR[0] = Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]) ? 0xFFFFFFFFU : 0U;
            ptrR[1] = Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]) ? 0xFFFFFFFFU : 0U;
            ptrR[2] = Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]) ? 0xFFFFFFFFU : 0U;
            ptrR[3] = Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]) ? 0xFFFFFFFFU : 0U;
        }

        return result.AsSingle();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_nlt_us_ps___cmp_nlt_uq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareGreaterThanOrEqual(a, b).AsUInt32();
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_nle_us_ps___cmp_nle_uq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareGreaterThan(a, b).AsUInt32();
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_ord_q_ps(Vector128<Single> a, Vector128<Single> b)
    {
        Vector128<uint> result;
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            ptrR[0] = !Single.IsNaN(ptrA[0]) && !Single.IsNaN(ptrB[0]) ? 0xFFFFFFFFU : 0U;
            ptrR[1] = !Single.IsNaN(ptrA[1]) && !Single.IsNaN(ptrB[1]) ? 0xFFFFFFFFU : 0U;
            ptrR[2] = !Single.IsNaN(ptrA[2]) && !Single.IsNaN(ptrB[2]) ? 0xFFFFFFFFU : 0U;
            ptrR[3] = !Single.IsNaN(ptrA[3]) && !Single.IsNaN(ptrB[3]) ? 0xFFFFFFFFU : 0U;
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_nge_us_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareLessThan(a, b).AsUInt32();

        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_ngt_us_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareLessThanOrEqual(b, a).AsUInt32();

        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_neq_oq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareEqual(a, b).AsUInt32();
        result = AdvSimd.Not(result);
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0U;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0U;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0U;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0U;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_unord_s_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = Vector128.Create(0U);

        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_neq_us_ps___cmp_neq_uq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareEqual(a, b);
        return AdvSimd.Not(result);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_ord_s_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = Vector128.Create(0U);

        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (!Single.IsNaN(ptrA[0]) && !Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (!Single.IsNaN(ptrA[1]) && !Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (!Single.IsNaN(ptrA[2]) && !Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (!Single.IsNaN(ptrA[3]) && !Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_eq_us_ps__cmp_eq_uq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareEqual(a, b).AsUInt32();
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;
            
            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_nge_uq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareLessThan(a, b).AsUInt32();
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_ngt_uq_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareLessThanOrEqual(b, a).AsUInt32();
        unsafe
        {
            var ptrA = (Single*)&a;
            var ptrB = (Single*)&b;
            var ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0xFFFFFFFFU;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0xFFFFFFFFU;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<Single> _cmp_neq_os_ps(Vector128<Single> a, Vector128<Single> b)
    {
        var result = AdvSimd.CompareEqual(a, b).AsUInt32();
        result = AdvSimd.Not(result);

        unsafe
        {
            Single* ptrA = (Single*)&a;
            Single* ptrB = (Single*)&b;
            uint* ptrR = (uint*)&result;

            if (Single.IsNaN(ptrA[0]) || Single.IsNaN(ptrB[0]))
            {
                ptrR[0] = 0U;
            }

            if (Single.IsNaN(ptrA[1]) || Single.IsNaN(ptrB[1]))
            {
                ptrR[1] = 0U;
            }

            if (Single.IsNaN(ptrA[2]) || Single.IsNaN(ptrB[2]))
            {
                ptrR[2] = 0U;
            }

            if (Single.IsNaN(ptrA[3]) || Single.IsNaN(ptrB[3]))
            {
                ptrR[3] = 0U;
            }
        }

        return result.AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<Single> Compare(Vector256<Single> a, Vector256<Single> b, FloatComparisonMode imm8)
    {
        var aLow = a.GetLower();
        var aHigh = a.GetUpper();
        var bLow = b.GetLower();
        var bHigh = b.GetUpper();
        Vector128<Single> resultLow;
        Vector128<Single> resultHigh;
        switch (imm8)
        {
            case FloatComparisonMode.OrderedEqualNonSignaling:
                resultLow = AdvSimd.CompareEqual(aLow, bLow);
                resultHigh = AdvSimd.CompareEqual(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedLessThanSignaling:
                resultLow = AdvSimd.CompareLessThan(aLow, bLow);
                resultHigh = AdvSimd.CompareLessThan(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedLessThanOrEqualSignaling:
                resultLow = AdvSimd.CompareLessThanOrEqual(aLow, bLow);
                resultHigh = AdvSimd.CompareLessThanOrEqual(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNonSignaling:
                resultLow = _cmp_unord_q_ps(aLow, bLow);
                resultHigh = _cmp_unord_q_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotEqualNonSignaling:
                resultLow = _cmp_neq_us_ps___cmp_neq_uq_ps(aLow, bLow);
                resultHigh = _cmp_neq_us_ps___cmp_neq_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotLessThanSignaling:
                resultLow = _cmp_nlt_us_ps___cmp_nlt_uq_ps(aLow, bLow);
                resultHigh = _cmp_nlt_us_ps___cmp_nlt_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotLessThanOrEqualSignaling:
                resultLow = _cmp_nle_us_ps___cmp_nle_uq_ps(aLow, bLow);
                resultHigh = _cmp_nle_us_ps___cmp_nle_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedNonSignaling:
                resultLow = _cmp_ord_q_ps(aLow, bLow);
                resultHigh = _cmp_ord_q_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedEqualNonSignaling:
                resultLow = _cmp_eq_us_ps__cmp_eq_uq_ps(aLow, bLow);
                resultHigh = _cmp_eq_us_ps__cmp_eq_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotGreaterThanOrEqualSignaling:
                resultLow = _cmp_nge_us_ps(aLow, bLow);
                resultHigh = _cmp_nge_us_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotGreaterThanSignaling:
                resultLow = _cmp_ngt_us_ps(aLow, bLow);
                resultHigh = _cmp_ngt_us_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedFalseNonSignaling:
                return Vector256.Create<Single>(0);
            case FloatComparisonMode.OrderedNotEqualNonSignaling:
                resultLow = _cmp_neq_oq_ps(aLow, bLow);
                resultHigh = _cmp_neq_oq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedGreaterThanOrEqualSignaling:
                resultLow = AdvSimd.CompareGreaterThanOrEqual(aLow, bLow);
                resultHigh = AdvSimd.CompareGreaterThanOrEqual(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedGreaterThanSignaling:
                resultLow = AdvSimd.CompareGreaterThan(aLow, bLow);
                resultHigh = AdvSimd.CompareGreaterThan(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedTrueNonSignaling:
                return Vector256.Create<Single>(-1);
            case FloatComparisonMode.OrderedEqualSignaling:
                resultLow = AdvSimd.CompareEqual(aLow, bLow);
                resultHigh = AdvSimd.CompareEqual(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedLessThanNonSignaling:
                resultLow = AdvSimd.CompareLessThan(aLow, bLow);
                resultHigh = AdvSimd.CompareLessThan(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedLessThanOrEqualNonSignaling:
                resultLow = AdvSimd.CompareLessThanOrEqual(aLow, bLow);
                resultHigh = AdvSimd.CompareLessThanOrEqual(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedSignaling:
                resultLow = _cmp_unord_s_ps(aLow, bLow);
                resultHigh = _cmp_unord_s_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotEqualSignaling:
                resultLow = _cmp_neq_us_ps___cmp_neq_uq_ps(aLow, bLow);
                resultHigh = _cmp_neq_us_ps___cmp_neq_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotLessThanNonSignaling:
                resultLow = _cmp_nlt_us_ps___cmp_nlt_uq_ps(aLow, bLow);
                resultHigh = _cmp_nlt_us_ps___cmp_nlt_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotLessThanOrEqualNonSignaling:
                resultLow = _cmp_nle_us_ps___cmp_nle_uq_ps(aLow, bLow);
                resultHigh = _cmp_nle_us_ps___cmp_nle_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedSignaling:
                resultLow = _cmp_ord_s_ps(aLow, bLow);
                resultHigh = _cmp_ord_s_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedEqualSignaling:
                resultLow = _cmp_eq_us_ps__cmp_eq_uq_ps(aLow, bLow);
                resultHigh = _cmp_eq_us_ps__cmp_eq_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotGreaterThanOrEqualNonSignaling:
                resultLow = _cmp_nge_uq_ps(aLow, bLow);
                resultHigh = _cmp_nge_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedNotGreaterThanNonSignaling:
                resultLow = _cmp_ngt_uq_ps(aLow, bLow);
                resultHigh = _cmp_ngt_uq_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedFalseSignaling:
                return Vector256.Create<Single>(0);
            case FloatComparisonMode.OrderedNotEqualSignaling:
                resultLow = _cmp_neq_os_ps(aLow, bLow);
                resultHigh = _cmp_neq_os_ps(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling:
                resultLow = AdvSimd.CompareGreaterThanOrEqual(aLow, bLow);
                resultHigh = AdvSimd.CompareGreaterThanOrEqual(aHigh, bHigh);
                break;
            case FloatComparisonMode.OrderedGreaterThanNonSignaling:
                resultLow = AdvSimd.CompareGreaterThan(aLow, bLow);
                resultHigh = AdvSimd.CompareGreaterThan(aHigh, bHigh);
                break;
            case FloatComparisonMode.UnorderedTrueSignaling:
                return Vector256.Create<Single>(-1);
            default:
                throw new ArgumentOutOfRangeException(nameof(imm8), imm8, null);
        }

        return Vector256.Create(resultLow, resultHigh);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TestZ(Vector256<uint> left, Vector256<uint> right)
    {
        var res = Vector256.Create(AdvSimd.And(left.GetLower(), right.GetLower()),
            AdvSimd.And(left.GetUpper(), right.GetUpper()));
        var tmp = AdvSimd.Or(res.GetLower(), res.GetUpper());
        return ~(AdvSimd.Extract(tmp, 0) | AdvSimd.Extract(tmp, 1)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Store(uint* address, Vector256<uint> source)
    {
        AdvSimd.Store(address, source.GetLower());
        AdvSimd.Store(address + 2, source.GetUpper());
    }
}