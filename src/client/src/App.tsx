import { Login, ChatRoom } from "./pages/_index";
import { Box, Container, Snackbar, Alert, Tabs, Tab } from "@mui/material";
import { useState, useEffect, useContext } from "react";
import { HubContext, UserContextProvider } from "./contexts/_index";
import IsportTableView from "./pages/Isport/DataView";

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function CustomTabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          <>{children}</>
        </Box>
      )}
    </div>
  );
}

const App = () => {
  const hubCtx = useContext(HubContext);

  const [inRoom, setInRoom] = useState(false);
  const [alertOpen, setAlertOpen] = useState(false);
  const [value, setValue] = useState(0);
  const handleChange = (event: React.SyntheticEvent, newValue: number) => {
    setValue(newValue);
  };

  useEffect(() => {
    if (hubCtx?.connectionStarted) {
      hubCtx?.connection?.onclose(() => {
        setInRoom(false);
      });
    }
  }, [hubCtx?.connection]);

  function a11yProps(index: number) {
    return {
      id: `simple-tab-${index}`,
      "aria-controls": `simple-tabpanel-${index}`,
    };
  }

  return (
    <>
      <UserContextProvider>
        <Box sx={{ width: "100%", bgcolor: "background.paper", borderColor: 'divider' }}>
          <Tabs value={value} onChange={handleChange} centered>
            <Tab label="Chat Room" {...a11yProps(0)} />
            <Tab label="Isport" {...a11yProps(1)} />
          </Tabs>
        </Box>
        <Box sx={{ width: "100%", height: "100vh" }}>
          <Container>
            <Box
              sx={{
                minWidth: "100%",
                minHeight: "100vh",
                padding: "20px",
                display: "flex",
                flexDirection: "column",
                justifyContent: "center",
                alignItems: "center",
              }}
            >
              {hubCtx?.connection !== null && (
                <>
                  <CustomTabPanel value={value} index={0}>
                    {inRoom ? (
                      <ChatRoom />
                    ) : (
                      <Login
                        onJoined={(success) => {
                          if (!success) {
                            setAlertOpen(true);
                          }
                          setInRoom(success);
                        }}
                      />
                    )}
                  </CustomTabPanel>
                  <CustomTabPanel value={value} index={1}>
                    <IsportTableView />
                  </CustomTabPanel>
                </>
              )}
            </Box>
          </Container>
        </Box>
      </UserContextProvider>

      <Snackbar open={alertOpen} autoHideDuration={1500}>
        <Alert severity="error" sx={{ width: "100%" }} variant="filled">
          Couldn't join the room!
        </Alert>
      </Snackbar>
    </>
  );
};

export default App;
