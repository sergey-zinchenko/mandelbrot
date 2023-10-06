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
    const __m256i IdentVector = _mm256_set1_epi32(1);
    const __m256 FourVec = _mm256_set1_ps(4.0f);
    const __m256 TwoVec = _mm256_set1_ps(2.0f);
    const __m256 DisplacementVector = _mm256_set_ps(7.0f, 6.0f, 5.0f, 4.0f, 3.0f, 2.0f, 1.0f, 0.0f);

    const int offset = h * WIDTH;
    const int mirrorOffset = (HEIGHT - h - 1) * WIDTH;
    for (int w = 0; w < WIDTH; w += 16) {
    const __m256 cyVec = _mm256_add_ps(MinYVec, _mm256_mul_ps(_mm256_set1_ps((float) h), ScaleYVec));
    const __m256 ScaleXDisplacementVec = _mm256_mul_ps(DisplacementVector, ScaleXVec);
    const __m256 MIN_XVec = _mm256_set1_ps(MIN_X);
    const __m256 wScaleXVec = _mm256_set1_ps((float) w * SCALE_X);
    const __m256 w8ScaleXVec = _mm256_set1_ps((float) (w + 8) * SCALE_X);
    const __m256 cxVec1 = _mm256_add_ps(_mm256_add_ps(MIN_XVec, wScaleXVec), ScaleXDisplacementVec);
    const __m256 cxVec2 = _mm256_add_ps(_mm256_add_ps(MIN_XVec, w8ScaleXVec), ScaleXDisplacementVec);
    __m256 zReVec1 = _mm256_setzero_ps();
    __m256 zImVec1 = _mm256_setzero_ps();
    __m256 zReVec2 = _mm256_setzero_ps();
    __m256 zImVec2 = _mm256_setzero_ps();
    __m256i nvVec1 = _mm256_setzero_si256();
    __m256i nvVec2 = _mm256_setzero_si256();
    __m256i breakVec1 = _mm256_setzero_si256();
    __m256i breakVec2 = _mm256_setzero_si256();
    __m256 mag2Vec1;
    __m256 mag2Vec2;
    int i1 = 0;
    int i2 = 0;
    do {
        __m256 zReNewVec1 = _mm256_mul_ps(zReVec1, zReVec1);
        __m256 zImNewVec1 = _mm256_mul_ps(zImVec1, zReVec1);
        __m256 zImVec1_pow2 = _mm256_mul_ps(zImVec1, zImVec1);
        __m256 zReNewVec2 = _mm256_mul_ps(zReVec2, zReVec2);
        __m256 zImNewVec2 = _mm256_mul_ps(zImVec2, zReVec2);
        __m256 zImVec2_pow2 = _mm256_mul_ps(zImVec2, zImVec2);

        zReNewVec1 = _mm256_sub_ps(zReNewVec1, zImVec1_pow2);
        zReNewVec2 = _mm256_sub_ps(zReNewVec2, zImVec2_pow2);

        zReNewVec1 = _mm256_add_ps(zReNewVec1, cxVec1);
        zReNewVec2 = _mm256_add_ps(zReNewVec2, cxVec2);
        zImNewVec1 = _mm256_add_ps(zImNewVec1, zImNewVec1);
        zImNewVec2 = _mm256_add_ps(zImNewVec2, zImNewVec2);
        zImNewVec1 = _mm256_add_ps(zImNewVec1, cyVec);
        zImNewVec2 = _mm256_add_ps(zImNewVec2, cyVec);
        
        __m256 zReNewVec1_pow2 = _mm256_mul_ps(zReNewVec1, zReNewVec1);
        __m256 zImNewVec1_pow2 = _mm256_mul_ps(zImNewVec1, zImNewVec1);
        __m256 zReNewVec2_pow2 = _mm256_mul_ps(zReNewVec2, zReNewVec2);
        __m256 zImNewVec2_pow2 = _mm256_mul_ps(zImNewVec2, zImNewVec2);

        mag2Vec1 = _mm256_add_ps(zReNewVec1_pow2, zImNewVec1_pow2);
        mag2Vec2 = _mm256_add_ps(zReNewVec2_pow2, zImNewVec2_pow2);

                     
        __m256 cmpVec1 = _mm256_cmp_ps(mag2Vec1, FourVec, _CMP_LT_OQ);
        __m256 cmpVec2 = _mm256_cmp_ps(mag2Vec2, FourVec, _CMP_LT_OQ);

        __m256i breakVec1_epi32 = _mm256_castps_si256(cmpVec1);
        __m256i breakVec2_epi32 = _mm256_castps_si256(cmpVec2);
        breakVec1_epi32 = _mm256_add_epi32(breakVec1_epi32, IdentVector);
        breakVec2_epi32 = _mm256_add_epi32(breakVec2_epi32, IdentVector);
        const __m256i nvVec1_epi32 = _mm256_andnot_si256(breakVec1_epi32, IdentVector);
        const __m256i nvVec2_epi32 = _mm256_andnot_si256(breakVec2_epi32, IdentVector);
        nvVec1 = _mm256_add_epi32(nvVec1_epi32, nvVec1);
        nvVec2 = _mm256_add_epi32(nvVec2_epi32, nvVec2);


        zReVec1 = _mm256_add_ps(
            _mm256_sub_ps(_mm256_mul_ps(zReNewVec1, zReNewVec1), _mm256_mul_ps(zImNewVec1, zImNewVec1)), cxVec1);
        zReVec2 = _mm256_add_ps(
            _mm256_sub_ps(_mm256_mul_ps(zReNewVec2, zReNewVec2), _mm256_mul_ps(zImNewVec2, zImNewVec2)), cxVec2);
        zImVec1 = _mm256_add_ps(_mm256_mul_ps(_mm256_mul_ps(zReNewVec1, zImNewVec1), TwoVec), cyVec);
        zImVec2 = _mm256_add_ps(_mm256_mul_ps(_mm256_mul_ps(zReNewVec2, zImNewVec2), TwoVec), cyVec);
        mag2Vec1 = _mm256_add_ps(_mm256_mul_ps(zReVec1, zReVec1),
                          _mm256_mul_ps(zImVec1, zImVec1));
        mag2Vec2 = _mm256_add_ps(_mm256_mul_ps(zReVec2, zReVec2),
                     _mm256_mul_ps(zImVec2, zImVec2));
        cmpVec1 = _mm256_cmp_ps(mag2Vec1, FourVec, _CMP_LT_OQ);
        cmpVec2 = _mm256_cmp_ps(mag2Vec2, FourVec, _CMP_LT_OQ);
        breakVec1 = _mm256_or_si256(_mm256_add_epi32(_mm256_castps_si256(cmpVec1), IdentVector), breakVec1);
        breakVec2 = _mm256_or_si256(_mm256_add_epi32(_mm256_castps_si256(cmpVec2), IdentVector), breakVec2);
        nvVec1 = _mm256_add_epi32(nvVec1, _mm256_andnot_si256(breakVec1, IdentVector));
        nvVec2 = _mm256_add_epi32(nvVec2, _mm256_andnot_si256(breakVec2, IdentVector));
        i1+=2;
        i2+=2;
    } while ((!_mm256_testz_si256(_mm256_andnot_si256(breakVec1, IdentVector), IdentVector) && i1 < MAX_ITERS) ||
         (!_mm256_testz_si256(_mm256_andnot_si256(breakVec2, IdentVector), IdentVector) && i2 < MAX_ITERS));
    _mm256_storeu_si256((__m256i *) (pResult + offset + w), nvVec1);
    _mm256_storeu_si256((__m256i *) (pResult + mirrorOffset + w), nvVec1);
    _mm256_storeu_si256((__m256i *) (pResult + offset + w + 8), nvVec2);
    _mm256_storeu_si256((__m256i *) (pResult + mirrorOffset + w + 8), nvVec2);
    }
}

void MandelbrotSimd(uint *pResult) {
#pragma omp parallel for num_threads(4 * NUM_CPU) schedule(static) shared(pResult) default(none)
    for (int h = 0; h < HEIGHT / 2; h++) {
        Mandelbrot_0_simd(h, pResult);
    }
}

int main() {
    printf("NumCpu : %d\n", NUM_CPU);
    printf("AVX supported: %s\n", _mm256_testz_si256(_mm256_setzero_si256(), _mm256_setzero_si256()) ? "yes" : "no");
    uint *pResult = (uint *) calloc(HEIGHT * WIDTH, sizeof(uint));


    const int NUM_ITERATIONS = 10;
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