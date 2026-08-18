#pragma once

#include <Arduino.h>

inline float moisturePercentFromAdc(int rawAdc, int dryAdc, int wetAdc)
{
    const float percentage =
        100.0f * static_cast<float>(rawAdc - dryAdc) /
        static_cast<float>(wetAdc - dryAdc);

    return constrain(percentage, 0.0f, 100.0f);
}
