#include "vignette.h"
#include "pch.h"
#include <cmath>
#include <cstdlib>

extern "C" __declspec(dllexport)
int Myproc(int a, int b)
{
	return a + b;
}

extern "C" __declspec(dllexport)
void Vignette(unsigned char* pixelArray, double* pixelArrayMask, int width, int height, double force, double vignetteRadius)
{
	int centerX = width / 2;
	int centerY = height / 2;
	div_t wynik;
	double imageRadius = sqrt(pow(width, 2) + pow(height, 2));

	for (int i = 0; i < (width * height); i++)
	{
		wynik = div(i, width);
		//pixelArrayMask[i] = 1 - pow((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius, force);
		pixelArrayMask[i] = 1 / (1 + pow(2.71828182845904, force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius)));
	}

	for (int i = 0; i < width * height * 3; ++i)
	{
		pixelArray[i] *= pixelArrayMask[i / 3];
	}
	//pixelArray[0] *= 0.5;
	//pixelArray[1] *= 0.5;
	//pixelArray[2] *= 0.5;
}