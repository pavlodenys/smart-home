export interface SensorData {
    id: string;
    name: string;
    type: string;
    description: string;
    chartData: ChartData;
}

export interface DeviceData{
    id: number;
    name: string;
    status: string;
}

export interface ChartData {
    id: string;
    name: string;
    type: string;
    labels: string[];
    data: PointDto[];
}

export interface PointDto {
    id: number;
    dateTime: string;
    name: string;
    value: number;
}

export const ComparisonOperator = {
    GreaterThan: 0,
    LessThan: 1,
    Equal: 2,
    NotEqual: 3,
    GreaterThanOrEqual: 4,
    LessThanOrEqual: 5 
}
