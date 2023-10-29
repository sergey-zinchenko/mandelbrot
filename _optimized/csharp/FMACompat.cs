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

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace ConsoleApp;

public class FmaCompat: AvxCompat
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MultiplyAdd(Vector256<float> a, Vector256<float> b, Vector256<float> c)
    {

        return Vector256.Create(AdvSimd.FusedMultiplyAdd(a.GetLower(), b.GetLower(), c.GetLower()),
            AdvSimd.FusedMultiplyAdd(a.GetUpper(), b.GetUpper(), c.GetUpper()));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MultiplySubtract(Vector256<float> a, Vector256<float> b, Vector256<float> c)
    {
        return Vector256.Create(AdvSimd.FusedMultiplySubtract(a.GetLower(), b.GetLower(), c.GetLower()),
            AdvSimd.FusedMultiplySubtract(a.GetUpper(), b.GetUpper(), c.GetUpper()));
    }
}