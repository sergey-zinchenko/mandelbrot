#include <stdio.h>
#include <stdlib.h>
#include <immintrin.h>
#include <omp.h>
#include <memory.h>
#include <time.h>
#include <math.h>

typedef unsigned int uint;
#define HEIGHT 1024
#define WIDTH 1024
#define MIN_X (-2.0f)
#define MAX_X 0.47f
#define MIN_Y (-1.12f)
#define MAX_Y 1.12f
#define SCALE_X ((MAX_X - MIN_X) / WIDTH)
#define SCALE_Y ((MAX_Y - MIN_Y) / HEIGHT)
#define MAX_ITERS 256
#define NUM_CPU omp_get_num_procs()
#define OFFSET(h, w) ((h) * WIDTH + (w))

void inline __attribute__((always_inline)) Mandelbrot_0_simd(int h, uint *pResult) {

    const __m256 MinYVec = _mm256_set1_ps(MIN_Y);
    const __m256 ScaleXVec = _mm256_set1_ps(SCALE_X);
    const __m256 ScaleYVec = _mm256_set1_ps(SCALE_Y);
    const __m256 FourVec = _mm256_set1_ps(4.0f);
    const __m256 DisplacementVector = _mm256_set_ps(7.0f, 6.0f, 5.0f, 4.0f, 3.0f, 2.0f, 1.0f, 0.0f);
    const __m256i IdentVector = _mm256_set1_epi32(1);

    const int offset = h * WIDTH;
    const int mirrorOffset = (HEIGHT - h - 1) * WIDTH;

    for (int w = 0; w < WIDTH; w += 32) {

        const __m256 cxVec1set = _mm256_set1_ps(MIN_X + (float) w * SCALE_X);
        const __m256 cxVec2set = _mm256_set1_ps(MIN_X + (float) (w + 8) * SCALE_X);
        const __m256 cxVec3set = _mm256_set1_ps(MIN_X + (float) (w + 16) * SCALE_X);
        const __m256 cxVec4set = _mm256_set1_ps(MIN_X + (float) (w + 24) * SCALE_X);
        const __m256 cyVecSet = _mm256_set1_ps((float) h);
        __m256 cxVec1 = _mm256_fmadd_ps(DisplacementVector, ScaleXVec, cxVec1set);
        __m256 cxVec2 = _mm256_fmadd_ps(DisplacementVector, ScaleXVec, cxVec2set);
        __m256 cxVec3 = _mm256_fmadd_ps(DisplacementVector, ScaleXVec, cxVec3set);
        __m256 cxVec4 = _mm256_fmadd_ps(DisplacementVector, ScaleXVec, cxVec4set);
        __m256 cyVec = _mm256_fmadd_ps(cyVecSet,ScaleYVec, MinYVec);
        __m256 zReVec1 = _mm256_setzero_ps();
        __m256 zImVec1 = _mm256_setzero_ps();
        __m256 zReVec2 = _mm256_setzero_ps();
        __m256 zImVec2 = _mm256_setzero_ps();
        __m256 zReVec3 = _mm256_setzero_ps();
        __m256 zImVec3 = _mm256_setzero_ps();
        __m256 zReVec4 = _mm256_setzero_ps();
        __m256 zImVec4 = _mm256_setzero_ps();
        __m256i nvVec1 = _mm256_setzero_si256();
        __m256i nvVec2 = _mm256_setzero_si256();
        __m256i nvVec3 = _mm256_setzero_si256();
        __m256i nvVec4 = _mm256_setzero_si256();
        __m256i breakVec1 = _mm256_setzero_si256();
        __m256i breakVec2 = _mm256_setzero_si256();
        __m256i breakVec3 = _mm256_setzero_si256();
        __m256i breakVec4 = _mm256_setzero_si256();
        __m256 mag2Vec1;
        __m256 mag2Vec2;
        __m256 mag2Vec3;
        __m256 mag2Vec4;

        int i = 0;

        do {
            __m256 zImVec1_pow2 = _mm256_mul_ps(zImVec1, zImVec1);
            __m256 zImVec2_pow2 = _mm256_mul_ps(zImVec2, zImVec2);
            __m256 zImVec3_pow2 = _mm256_mul_ps(zImVec3, zImVec3);
            __m256 zImVec4_pow2 = _mm256_mul_ps(zImVec4, zImVec4);

            __m256 zReNewVec1 = _mm256_fmsub_ps(zReVec1, zReVec1, zImVec1_pow2);
            __m256 zReNewVec2 = _mm256_fmsub_ps(zReVec2, zReVec2, zImVec2_pow2);
            __m256 zReNewVec3 = _mm256_fmsub_ps(zReVec3, zReVec3, zImVec3_pow2);
            __m256 zReNewVec4 = _mm256_fmsub_ps(zReVec4, zReVec4, zImVec4_pow2);

            __m256 zImNewVec1 = _mm256_mul_ps(zImVec1, zReVec1);
            __m256 zImNewVec2 = _mm256_mul_ps(zImVec2, zReVec2);
            __m256 zImNewVec3 = _mm256_mul_ps(zImVec3, zReVec3);
            __m256 zImNewVec4 = _mm256_mul_ps(zImVec4, zReVec4);

            zReNewVec1 = _mm256_add_ps(zReNewVec1, cxVec1);
            zReNewVec2 = _mm256_add_ps(zReNewVec2, cxVec2);
            zReNewVec3 = _mm256_add_ps(zReNewVec3, cxVec3);
            zReNewVec4 = _mm256_add_ps(zReNewVec4, cxVec4);

            zImNewVec1 = _mm256_add_ps(zImNewVec1, zImNewVec1);
            zImNewVec2 = _mm256_add_ps(zImNewVec2, zImNewVec2);
            zImNewVec3 = _mm256_add_ps(zImNewVec3, zImNewVec3);
            zImNewVec4 = _mm256_add_ps(zImNewVec4, zImNewVec4);

            zImNewVec1 = _mm256_add_ps(zImNewVec1, cyVec);
            zImNewVec2 = _mm256_add_ps(zImNewVec2, cyVec);
            zImNewVec3 = _mm256_add_ps(zImNewVec3, cyVec);
            zImNewVec4 = _mm256_add_ps(zImNewVec4, cyVec);

            __m256 zReNewVec1_pow2 = _mm256_mul_ps(zReNewVec1, zReNewVec1);
            __m256 zImNewVec1_pow2 = _mm256_mul_ps(zImNewVec1, zImNewVec1);
            __m256 zReNewVec2_pow2 = _mm256_mul_ps(zReNewVec2, zReNewVec2);
            __m256 zImNewVec2_pow2 = _mm256_mul_ps(zImNewVec2, zImNewVec2);
            __m256 zReNewVec3_pow2 = _mm256_mul_ps(zReNewVec3, zReNewVec3);
            __m256 zImNewVec3_pow2 = _mm256_mul_ps(zImNewVec3, zImNewVec3);
            __m256 zReNewVec4_pow2 = _mm256_mul_ps(zReNewVec4, zReNewVec4);
            __m256 zImNewVec4_pow2 = _mm256_mul_ps(zImNewVec4, zImNewVec4);

            mag2Vec1 = _mm256_add_ps(zReNewVec1_pow2, zImNewVec1_pow2);
            mag2Vec2 = _mm256_add_ps(zReNewVec2_pow2, zImNewVec2_pow2);
            mag2Vec3 = _mm256_add_ps(zReNewVec3_pow2, zImNewVec3_pow2);
            mag2Vec4 = _mm256_add_ps(zReNewVec4_pow2, zImNewVec4_pow2);

            __m256 cmpVec1 = _mm256_cmp_ps(mag2Vec1, FourVec, _CMP_LT_OQ);
            __m256 cmpVec2 = _mm256_cmp_ps(mag2Vec2, FourVec, _CMP_LT_OQ);
            __m256 cmpVec3 = _mm256_cmp_ps(mag2Vec3, FourVec, _CMP_LT_OQ);
            __m256 cmpVec4 = _mm256_cmp_ps(mag2Vec4, FourVec, _CMP_LT_OQ);

            __m256i breakVec1_epi32 = _mm256_castps_si256(cmpVec1);
            __m256i breakVec2_epi32 = _mm256_castps_si256(cmpVec2);
            __m256i breakVec3_epi32 = _mm256_castps_si256(cmpVec3);
            __m256i breakVec4_epi32 = _mm256_castps_si256(cmpVec4);

            breakVec1_epi32 = _mm256_add_epi32(breakVec1_epi32, IdentVector);
            breakVec2_epi32 = _mm256_add_epi32(breakVec2_epi32, IdentVector);
            breakVec3_epi32 = _mm256_add_epi32(breakVec3_epi32, IdentVector);
            breakVec4_epi32 = _mm256_add_epi32(breakVec4_epi32, IdentVector);

            breakVec1 =_mm256_or_si256(breakVec1, breakVec1_epi32);
            breakVec2 =_mm256_or_si256(breakVec2, breakVec2_epi32);
            breakVec3 =_mm256_or_si256(breakVec3, breakVec3_epi32);
            breakVec4 =_mm256_or_si256(breakVec4, breakVec4_epi32);

            __m256i nvVec1_epi32 = _mm256_andnot_si256(breakVec1, IdentVector);
            __m256i nvVec2_epi32 = _mm256_andnot_si256(breakVec2, IdentVector);
            __m256i nvVec3_epi32 = _mm256_andnot_si256(breakVec3, IdentVector);
            __m256i nvVec4_epi32 = _mm256_andnot_si256(breakVec4, IdentVector);

            nvVec1 = _mm256_add_epi32(nvVec1_epi32, nvVec1);
            nvVec2 = _mm256_add_epi32(nvVec2_epi32, nvVec2);
            nvVec3 = _mm256_add_epi32(nvVec3_epi32, nvVec3);
            nvVec4 = _mm256_add_epi32(nvVec4_epi32, nvVec4);

        ///

            zImNewVec1_pow2 = _mm256_mul_ps(zImNewVec1, zImNewVec1);
            zImNewVec2_pow2 = _mm256_mul_ps(zImNewVec2, zImNewVec2);
            zImNewVec3_pow2 = _mm256_mul_ps(zImNewVec3, zImNewVec3);
            zImNewVec4_pow2 = _mm256_mul_ps(zImNewVec4, zImNewVec4);

            zReVec1 = _mm256_fmsub_ps(zReNewVec1, zReNewVec1, zImNewVec1_pow2);
            zReVec2 = _mm256_fmsub_ps(zReNewVec2, zReNewVec2, zImNewVec2_pow2);
            zReVec3 = _mm256_fmsub_ps(zReNewVec3, zReNewVec3, zImNewVec3_pow2);
            zReVec4 = _mm256_fmsub_ps(zReNewVec4, zReNewVec4, zImNewVec4_pow2);

            zImVec1 = _mm256_mul_ps(zImNewVec1, zReNewVec1);
            zImVec2 = _mm256_mul_ps(zImNewVec2, zReNewVec2);
            zImVec3 = _mm256_mul_ps(zImNewVec3, zReNewVec3);
            zImVec4 = _mm256_mul_ps(zImNewVec4, zReNewVec4);

            zReVec1 = _mm256_add_ps(zReVec1, cxVec1);
            zReVec2 = _mm256_add_ps(zReVec2, cxVec2);
            zReVec3 = _mm256_add_ps(zReVec3, cxVec3);
            zReVec4 = _mm256_add_ps(zReVec4, cxVec4);

            zImVec1 = _mm256_add_ps(zImVec1, zImVec1);
            zImVec2 = _mm256_add_ps(zImVec2, zImVec2);
            zImVec3 = _mm256_add_ps(zImVec3, zImVec3);
            zImVec4 = _mm256_add_ps(zImVec4, zImVec4);

            zImVec1 = _mm256_add_ps(zImVec1, cyVec);
            zImVec2 = _mm256_add_ps(zImVec2, cyVec);
            zImVec3 = _mm256_add_ps(zImVec3, cyVec);
            zImVec4 = _mm256_add_ps(zImVec4, cyVec);

            __m256 zReVec1_pow2 = _mm256_mul_ps(zReVec1, zReVec1);
            zImVec1_pow2 = _mm256_mul_ps(zImVec1, zImVec1);
            __m256 zReVec2_pow2 = _mm256_mul_ps(zReVec2, zReVec2);
            zImVec2_pow2 = _mm256_mul_ps(zImVec2, zImVec2);
            __m256 zReVec3_pow2 = _mm256_mul_ps(zReVec3, zReVec3);
            zImVec3_pow2 = _mm256_mul_ps(zImVec3, zImVec3);
            __m256 zReVec4_pow2 = _mm256_mul_ps(zReVec4, zReVec4);
            zImVec4_pow2 = _mm256_mul_ps(zImVec4, zImVec4);

            mag2Vec1 = _mm256_add_ps(zReVec1_pow2, zImVec1_pow2);
            mag2Vec2 = _mm256_add_ps(zReVec2_pow2, zImVec2_pow2);
            mag2Vec3 = _mm256_add_ps(zReVec3_pow2, zImVec3_pow2);
            mag2Vec4 = _mm256_add_ps(zReVec4_pow2, zImVec4_pow2);

            cmpVec1 = _mm256_cmp_ps(mag2Vec1, FourVec, _CMP_LT_OQ);
            cmpVec2 = _mm256_cmp_ps(mag2Vec2, FourVec, _CMP_LT_OQ);
            cmpVec3 = _mm256_cmp_ps(mag2Vec3, FourVec, _CMP_LT_OQ);
            cmpVec4 = _mm256_cmp_ps(mag2Vec4, FourVec, _CMP_LT_OQ);

            breakVec1_epi32 = _mm256_castps_si256(cmpVec1);
            breakVec2_epi32 = _mm256_castps_si256(cmpVec2);
            breakVec3_epi32 = _mm256_castps_si256(cmpVec3);
            breakVec4_epi32 = _mm256_castps_si256(cmpVec4);

            breakVec1_epi32 = _mm256_add_epi32(breakVec1_epi32, IdentVector);
            breakVec2_epi32 = _mm256_add_epi32(breakVec2_epi32, IdentVector);
            breakVec3_epi32 = _mm256_add_epi32(breakVec3_epi32, IdentVector);
            breakVec4_epi32 = _mm256_add_epi32(breakVec4_epi32, IdentVector);

            breakVec1 = _mm256_or_si256(breakVec1, breakVec1_epi32);
            breakVec2 = _mm256_or_si256(breakVec2, breakVec2_epi32);
            breakVec3 = _mm256_or_si256(breakVec3, breakVec3_epi32);
            breakVec4 = _mm256_or_si256(breakVec4, breakVec4_epi32);

            nvVec1_epi32 = _mm256_andnot_si256(breakVec1, IdentVector);
            nvVec2_epi32 = _mm256_andnot_si256(breakVec2, IdentVector);
            nvVec3_epi32 = _mm256_andnot_si256(breakVec3, IdentVector);
            nvVec4_epi32 = _mm256_andnot_si256(breakVec4, IdentVector);

            nvVec1 = _mm256_add_epi32(nvVec1_epi32, nvVec1);
            nvVec2 = _mm256_add_epi32(nvVec2_epi32, nvVec2);
            nvVec3 = _mm256_add_epi32(nvVec3_epi32, nvVec3);
            nvVec4 = _mm256_add_epi32(nvVec4_epi32, nvVec4);

            i += 2;
        } while ((!_mm256_testz_si256(_mm256_andnot_si256(breakVec1, IdentVector), IdentVector) ||
                 !_mm256_testz_si256(_mm256_andnot_si256(breakVec2, IdentVector), IdentVector) ||
                !_mm256_testz_si256(_mm256_andnot_si256(breakVec3, IdentVector), IdentVector) ||
                !_mm256_testz_si256(_mm256_andnot_si256(breakVec4, IdentVector), IdentVector) ) && i < MAX_ITERS);

        _mm256_storeu_si256((__m256i *) (pResult + offset + w), nvVec1);
        _mm256_storeu_si256((__m256i *) (pResult + mirrorOffset + w), nvVec1);
        _mm256_storeu_si256((__m256i *) (pResult + offset + w + 8), nvVec2);
        _mm256_storeu_si256((__m256i *) (pResult + mirrorOffset + w + 8), nvVec2);
        _mm256_storeu_si256((__m256i *) (pResult + offset + w + 16), nvVec3);
        _mm256_storeu_si256((__m256i *) (pResult + mirrorOffset + w + 16), nvVec3);
        _mm256_storeu_si256((__m256i *) (pResult + offset + w + 24), nvVec4);
        _mm256_storeu_si256((__m256i *) (pResult + mirrorOffset + w + 24), nvVec4);
    }
}

void MandelbrotSimd(uint *pResult) {
#pragma omp parallel for num_threads(NUM_CPU) schedule(static) shared(pResult) default(none)
    for (int h = 0; h < HEIGHT / 2; h++) {
        Mandelbrot_0_simd(h, pResult);
    }
}

int main() {
    printf("NumCpu : %d\n", NUM_CPU);
    printf("AVX supported: %s\n", _mm256_testz_si256(_mm256_setzero_si256(), _mm256_setzero_si256()) ? "yes" : "no");
    uint *pResult = (uint *) calloc(HEIGHT * WIDTH, sizeof(uint));


    const int NUM_ITERATIONS = 1000;
    double *measurements = (double *) malloc(NUM_ITERATIONS * sizeof(double));
    MandelbrotSimd(pResult);
    for (int i = 0; i < NUM_ITERATIONS; i++) {
        memset(pResult, 0, HEIGHT * WIDTH * sizeof(uint));
        struct timespec start;
        struct timespec end;
        clock_gettime(CLOCK_REALTIME, &start);
        MandelbrotSimd(pResult);
        clock_gettime(CLOCK_REALTIME, &end);
        double exec_time =
                (double) (end.tv_sec - start.tv_sec) * 1000.0 + (double) (end.tv_nsec - start.tv_nsec) / 1000000.0;
        measurements[i] = exec_time;
        uint sum = 0;
        for (int h = 0; h < HEIGHT; h++) {
            for (int w = 0; w < WIDTH; w++) {
                sum += pResult[OFFSET(h, w)];
            }
        }
        printf("Execution Time:      %fms\t  %u\n", exec_time, sum);
    }

    double average = 0.0;
    for (int i = 0; i < NUM_ITERATIONS; i++) {
        average += measurements[i];
    }
    average /= NUM_ITERATIONS;

    double sum_of_squares = 0.0;
    for (int i = 0; i < NUM_ITERATIONS; i++) {
        sum_of_squares += (measurements[i] - average) * (measurements[i] - average);
    }
    double standard_deviation = sqrt(sum_of_squares / (NUM_ITERATIONS - 1)) / average * 100.0;
    printf("Avg: %.2fms, StdDev: %.4f%%\n", average, standard_deviation);

    // write to file
    FILE *fp = fopen("output.txt", "w");
    if (fp == NULL) {
        printf("Error opening file!\n");
        exit(1);
    }
    for (int i = 0; i < HEIGHT * WIDTH - 1; i++) {
        if (fprintf(fp, "%d,", pResult[i]) < 0) {
            printf("Error writing to file!\n");
            exit(1);
        }
    }
    if (fprintf(fp, "%d", pResult[HEIGHT * WIDTH - 1]) < 0) {
        printf("Error writing to file!\n");
        exit(1);
    }
    fclose(fp);

    free(pResult);
    free(measurements);

    return 0;
}