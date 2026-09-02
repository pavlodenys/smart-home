export type ChartPointDate = {
  dateTime: string | Date;
};

export const getSensorDetailsUrl = (sensorId: number, date: string): string =>
  `api/sensor/${sensorId}/${date}`;

export const getFirstPoint = (points: ChartPointDate[]): Date => {
  if (points && points.length) {
    if (points.length > 30) {
      return new Date(
        new Date(points[points.length - 1].dateTime).getTime() - 20 * 60 * 1000,
      );
    }

    return new Date(points[0].dateTime);
  }

  return new Date();
};
