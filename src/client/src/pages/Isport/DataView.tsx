import * as React from "react";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import axios from "axios";
import { HubContext } from "../../contexts/HubContext";

const statusMapping = {
  "0": "Not started",
  "1": "First half",
  "2": "Half-time break",
  "3": "Second half",
  "4": "Extra time",
  "5": "Penalty",
  "-1": "Finished",
  "-10": "Cancelled",
  "-11": "TBD",
  "-12": "Terminated",
  "-13": "Interrupted",
  "-14": "Postponed",
};
function timeConverter(UNIX_timestamp: number){
  var a = new Date(UNIX_timestamp * 1000);
  var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
  var year = a.getFullYear();
  var month = months[a.getMonth()];
  var date = a.getDate();
  var hour = a.getHours();
  var min = a.getMinutes();
  var sec = a.getSeconds();
  var time = date + ' ' + month + ' ' + year + ' ' + hour + ':' + min + ':' + sec ;
  return time;
}
const columns: GridColDef[] = [
  { field: "matchId", headerName: "Match Id", width: 130 },
  {
    field: "startTime",
    headerName: "Start Time",
    width: 200,
    valueGetter: (value, row) =>
      `${timeConverter(value)}`,
  },
  {
    field: "matchTime",
    headerName: "Match Time",
    width: 200,
    valueGetter: (value, row) =>
      `${timeConverter(value)}`,
  },
  {
    field: "status",
    headerName: "Status",
    width: 130,
    valueGetter: (value, row) => `${statusMapping?.[value] ?? "unknown"}`,
  },
  { field: "homeScore", headerName: "Home Score" },
  { field: "awayScore", headerName: "Away Score" },
  { field: "homeHalfScore", headerName: "Home Half Score", width: 130 },
  { field: "awayHalfScore", headerName: "Away half Score", width: 130 },
  { field: "homeRed", headerName: "Home red" },
  { field: "awayRed", headerName: "Home red" },
  { field: "homeYellow", headerName: "Home yellow" },
  { field: "awayYellow", headerName: "Home yellow" },
  { field: "homeCorner", headerName: "Home corner" },
  { field: "awayCorner", headerName: "Home corner" },
  { field: "hasLineup", headerName: "Has line up" },
  { field: "explain", headerName: "Explan" },
  { field: "var", headerName: "Var" },
  { field: "injuryTime", headerName: "Injury Time" },
];

export default function IsportTableView() {
  const hubCtx = React.useContext(HubContext);

  const [data, setData] = useState([]);

  React.useEffect(() => {
    hubCtx?.connection
      ?.invoke("GetIsportData", {})
      .then((result) => {
        setData(
          JSON.parse(result)?.data.map((e: any, i: any) => ({
            id: e.matchId,
            ...e,
          })),
        );
        console.log(JSON.parse(result)?.data);
      })
      .catch((e: any) => {
        console.log("Get Isport Data failed! ERROR : ", e);
      });

    return () => {};
  }, []);

  return (
    <div style={{ height: 400, width: 1000 }}>
      <DataGrid
        rows={[...data]}
        columns={columns}
        initialState={{
          pagination: {
            paginationModel: { page: 0, pageSize: 5 },
          },
        }}
        pageSizeOptions={[5, 10]}
      />
    </div>
  );
}
