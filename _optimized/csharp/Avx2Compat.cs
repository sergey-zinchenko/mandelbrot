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

public class Avx2Compat: AvxCompat
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> Add(Vector256<uint> left, Vector256<uint> right)
    { 
        return Vector256.Create(
            AdvSimd.Add(left.GetLower(), right.GetLower()), 
            AdvSimd.Add(left.GetUpper(), right.GetUpper())
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> Or(Vector256<uint> left, Vector256<uint> right)
    { 
        return Vector256.Create(
            AdvSimd.Or(left.GetLower(), right.GetLower()), 
            AdvSimd.Or(left.GetUpper(), right.GetUpper())
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> AndNot(Vector256<uint> left, Vector256<uint> right)
    {
        return Vector256.Create(AdvSimd.BitwiseClear(left.GetLower(), right.GetLower()),
            AdvSimd.BitwiseClear(left.GetUpper(), right.GetUpper()));
    }
}