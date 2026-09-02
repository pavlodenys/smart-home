export interface SensorData {
    [x: string]: any;
    id?: number;
    name: string;
    type: string;
    description: string;
    chartData: ChartData[];
}

export interface DeviceData{
    id: number;
    name: string;
    description: string;
    isActive: boolean;
}

export interface ChartData {
    [x: string]: any;
   // id?: string;
    name?: string;
    type?: string;
    description?: string;
    labels?: string[];
    data?: PointDto[];
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
} as const;

export const ScenarioActionType = {
    Device: 0,
    Notification: 1,
} as const;

export interface ScenarioData {
    id: number;
    threshold: number;
    hysteresis: number;
    operator: number;
    actionType: number;
    command?: string;
    isConditionActive: boolean;
    lastTriggeredAt?: string;
    sensors: Array<{ sensorId: number; sensor?: SensorData }>;
    devices: Array<{ deviceId: number; device?: DeviceData }>;
}
